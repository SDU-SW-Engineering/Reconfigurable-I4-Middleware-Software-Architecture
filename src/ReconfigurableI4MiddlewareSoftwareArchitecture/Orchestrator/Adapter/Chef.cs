using Confluent.Kafka;
using I4ToolchainDotnetCore.Communication.Kafka;
using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchestrator.Adapter.Interfaces;
using Orchestrator.Adapter.Kafka;
using Orchestrator.Adapter.RecipeInterpretation;
using Orchestrator.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orchestrator.DomainObjects;
using KafkaMessage = Orchestrator.Adapter.Kafka.KafkaMessage;

namespace Orchestrator.Adapter
{
    public class Chef : IChef, IObserver<KafkaMessage>, IObservable<AvailabilityNotification>
    {
        private readonly II4Logger log;
        private bool currentlyExecutingRecipe;
        private bool stopExecution;
        private readonly string ORIGIN_ID = "i4.sdu.dk/Middleware/Orchestrator";
        private Dictionary<String, String> savedVariables;
        private List<INotifier> notifiers;
        private IKafkaMultiConsumer _multiConsumer;
        private ConcurrentDictionary<string, ConcurrentQueue<KafkaMessage>> topicContents;
        private I4ToolchainDotnetCore.Communication.Kafka.KafkaProducer producer;
        private IKafkaProducer _producer;
        List<IObserver<AvailabilityNotification>> availabilityNotificationObservers;
        private string _chefId;

        public Chef(II4Logger log, IKafkaProducer producer, IKafkaMultiConsumer multiConsumer, string chefId)
        {
            this.log = log;
            this._chefId = chefId;
            _producer = producer;
            savedVariables = new Dictionary<string, string>();
            currentlyExecutingRecipe = false;
            stopExecution = false;
            _multiConsumer = multiConsumer;
            availabilityNotificationObservers = new List<IObserver<AvailabilityNotification>>();
            _multiConsumer.Subscribe(this);
            topicContents = new ConcurrentDictionary<string, ConcurrentQueue<KafkaMessage>>();
        }

        public bool isCurrentlyExecutingRecipe()
        {
            return currentlyExecutingRecipe;
        }

        public void SetStopExecution(bool value)
        {
            stopExecution = value;
        }

        public async Task ExecuteRecipe(Recipe recipe, Product product)
        {
            try
            {
                currentlyExecutingRecipe = true;
                stopExecution = false;
                log.LogDebug(GetType(), "Chef {chefId}: Starting execution of recipe {recipeName} with orderId {orderId} and product: {productId}", _chefId,recipe.recipeName, product.orderId, product.id);
                topicContents.Clear();
                var topics = ExtractTopics(recipe);
                foreach (var topic in topics)
                {
                    topicContents[topic] = new ConcurrentQueue<KafkaMessage>(new List<KafkaMessage>() {});
                }
                _multiConsumer.StartConsumption(ExtractTopics(recipe));
                foreach (Step step in recipe.steps)
                {
                    log.LogDebug(GetType(), "Chef {chefId}:Starting execution of step {stepId} for product: {productId}", _chefId,step.stepId, product.id);
                    try
                    {
                        CheckForStop(product);
                        await ExecuteStep(step, product);
                        product.logs.Add($"{DateTime.Now}: finished step {step.stepId}");
                        log.LogDebug(GetType(), "Chef {chefId}:Finished execution of step {stepId} for product: {productId}", _chefId,step.stepId, product.id);

                    } catch (StepNotExecutedException ex) {
                        log.LogError(GetType(), "Chef {chefId}:The recipe {recipeName} was not executed successfully for product {productId} and order {orderId}, stopped at step {stepId}, ", recipe.recipeName, product.id, product.orderId, step.stepId);
                        product.error = $"The recipe {recipe.recipeName} was not executed sucessfully, stopped at step {step.stepId} -> {ex.Message}";
                        product.logs.Add($"{DateTime.Now}: stopped at step {step.stepId}");
                        //throw new RecipeNotExecutedException($"The recipe {recipe.recipeName} was not executed sucessfully, stopped at step {step.stepId} -> {ex.Message}", ex);
                    }
                }
                product.finished = true;
                currentlyExecutingRecipe = false;
                log.LogDebug(GetType(), "Chef {chefId}:Finished executing recipe: {finishedRecipe}, for product: {productId} on order: {orderId}", _chefId,recipe.recipeName, product.id, product.orderId);
                foreach (var observer in availabilityNotificationObservers)
                {
                    observer.OnNext(new AvailabilityNotification(){Chef = this});
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        private List<string> ExtractTopics(Recipe recipe)
        {
            var topics = new List<string>();
            foreach (Step step in recipe.steps)
            {
                topics.Add(step.topic);
            }
            return topics;
        }

        private void CheckForStop(Product product)
        {
            if (stopExecution)
            {
                log.LogError(GetType(), "Chef {chefId}: The execution of order {orderId} and product {product} was stopped", _chefId,product.orderId, product.id);
                throw new StepNotExecutedException($"The execution of order {product.orderId} was stopped.");
            }
        }

        private async Task ExecuteStep(Step step, Product product)
        {
            String operationId = Guid.NewGuid().ToString();
            try
            {
                await SendCommand(step.topic, step.command, operationId, product);
                await WaitForResponse(operationId, step.topic, step.response, step.saveReceivedParameters, 400);
            }
            catch (ExpectedParameterNotFoundException ex)
            {
                throw new StepNotExecutedException($"The command could not be sent -> {ex.Message}", ex);
            }
            catch (ResponseTimeoutException ex)
            {
                throw new StepNotExecutedException(
                    $"The response was not received in the specified amount of time -> {ex.Message}", ex);
            }
            catch (CommandNotExecutedSuccessfullyException ex)
            {
                throw new StepNotExecutedException(
                    $"The command with operation: {step.command.operation} was not executed successfully -> {ex.Message}",
                    ex);
            }
            catch (TimeoutException ex)
            {

            }
            catch (Exception e)
            {
                log.LogError(GetType(),$"EXCEPTION: {e.Message}");
            }
        }

        private void NotifyExternalAssets(Dictionary<String, String> notifications)
        {
            log.LogDebug(GetType(), "Notifying {notifiers}", string.Join(", ", notifications.Keys));
            foreach (string key in notifications.Keys)
            {
                notifiers.FindAll(x => x.GetKeyword() == key).ForEach(n => n.Notify(notifications.GetValueOrDefault(key)));
            }
        }

        private async Task SendCommand(string topic, Command command, string operationId, Product product)
        {
            await Task.Run(() =>
            {
                JObject jsonCommand = new JObject
                {
                    { "@id", operationId },
                    { "@type", "operation" },
                    { "aasOriginId", ORIGIN_ID },
                    { "aasTargetId", command.targetId },
                    { "orderId", product.orderId },
                    { "productId", product.id },
                    { "operation", command.operation }
                };
                JObject parameters = new JObject();
                parameters = AddReceivedParameters(command, parameters);
                parameters = AddParameters(command, parameters);
                jsonCommand.Add("parameters", parameters);
                log.LogDebug(GetType(), "Chef {chefId}:Sending command {commandMessage} to topic {commandTopic}", _chefId,jsonCommand.ToString(), topic);
                _producer.ProduceMessage(new List<string>() { topic }, jsonCommand);
            });
        }

        private JObject AddReceivedParameters(Command command, JObject parameters)
        {
            if (command.insertReceivedParameters != null && command.insertReceivedParameters.Count > 0)
            {
                foreach (string value in command.insertReceivedParameters)
                {
                    if (savedVariables.TryGetValue(value, out string savedVariable))
                    {
                        log.LogDebug(GetType(), "Chef {chefId}:inserting received parameter {expectedParameter} with value {receivedValue}", _chefId,value, savedVariable);
                        parameters.Add(value, savedVariable);
                    }
                    else
                    {
                        throw new ExpectedParameterNotFoundException($"The parameter {value} was not in the list of saved parameters");
                    }
                }
            }
            return parameters;
        }

        private JObject AddParameters(Command command, JObject parameters)
        {
            if (command.parameters != null && command.parameters.Count != 0)
            {
                foreach (string key in command.parameters.Keys)
                {
                    parameters.Add(key, command.parameters[key]);
                }
            }
            return parameters;
        }

        private async Task WaitForResponse(string operationId, string topic, Response expectedResponse, List<String> variablesToBeSaved, int timeOutSeconds)
        {
            await Task.Run(() =>
            {
                bool responseReceived = false;
                DateTime start = DateTime.Now;
                log.LogDebug(GetType(), "Chef {chefId}:Waiting for response for id: {id} with parameters: {expectedResponseParameters} on topic {responseTopic}",_chefId,operationId, String.Join(", ", expectedResponse.parameters), topic);


                while (!responseReceived && !stopExecution)
                {

                    KafkaMessage message;
                    try
                    {
                         
                        
                        if (!topicContents[topic].TryDequeue(out message))
                        {
                            continue;
                            //throw new ArgumentNullException("Message was null");
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        continue;
                    }
                    catch (ArgumentNullException ex)
                    {
                        continue;
                    }
                    catch (KeyNotFoundException ex)
                    {
                        continue;
                    }

                    //Console.WriteLine("received message: " + message.Value);
                    if (message.TimeStamp.AddSeconds(60) < DateTime.Now)
                    {
                        log.LogDebug(GetType(), "Chef {chefId}:Received message {receivedMessage}, but was too old", _chefId,message.Value);
                        continue;
                    }
                    try
                    {
                        JObject jsonMessage = JObject.Parse(message.Value);

                        if (!jsonMessage.Value<String>("aasOriginId").Equals(ORIGIN_ID) && jsonMessage.Value<String>("requestId").Equals(operationId))
                        {
                            log.LogDebug(GetType(), "Chef {chefId}:Received response with operationId: {receivedOperationId}", _chefId,operationId);
                            responseReceived = true;

                            Dictionary<String, String> receivedParametersLowerCase = new Dictionary<String, String>(
                                JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonMessage.Value<JObject>("response").ToString()),
                                StringComparer.OrdinalIgnoreCase);
                            bool containsCorrectParameters = ResponseContainsCorrectParameters(expectedResponse.parameters, receivedParametersLowerCase);
                            bool operationSuccess = ConfirmCommandSuccess(receivedParametersLowerCase);
                            if (!containsCorrectParameters || !operationSuccess)
                            {
                                throw new CommandNotExecutedSuccessfullyException(!containsCorrectParameters ?
                                    "The received response does not contain the correct parameters" :
                                    $"The operation was not successfull," +
                                    (jsonMessage.ContainsKey("response") ? jsonMessage["response:error"]: "no description"));
                            }
                            if (variablesToBeSaved.Count > 0) SaveVariables(receivedParametersLowerCase, variablesToBeSaved);
                        }
                    }
                    catch (JsonReaderException ex)
                    {
                        log.LogError(GetType(), ex, "Could not parse message");
                    }
                    catch (NullReferenceException ex)
                    {
                        log.LogError(GetType(), ex, "Could not find originid in message: {message}", message.Value);
                    }
                    if (DateTime.Now.Subtract(start).TotalSeconds >= timeOutSeconds) throw new ResponseTimeoutException($"The waiting for response with id {operationId} timed out after {timeOutSeconds} seconds");
                }
                log.LogDebug(GetType(), "Chef {chefId}:Response received successfully for command: {commandId}", _chefId, operationId);

                });
            
        }





        private void SaveVariables(Dictionary<String, String> receivedParameters, List<string> variablesToBeSaved)
        {
            foreach (String v in variablesToBeSaved)
            {
                if (receivedParameters.TryGetValue(v, out string value))
                {
                    log.LogVerbose(GetType(), "Found value {receivedValue} for parameter {receivedParameter}", value, v);
                    savedVariables[v] = value;
                }
                else
                {
                    log.LogError(GetType(), "The expected value for variable {variableToBeSaved} was not found", v);
                }
            }

        }

        private bool ResponseContainsCorrectParameters(Dictionary<String, String> expectedParameters, Dictionary<String, String> receivedParameters)
        {
            bool containsCorrectParameters = new List<String>(expectedParameters.Keys).TrueForAll(x => receivedParameters.ContainsKey(x));
            return containsCorrectParameters;
        }

        private bool ConfirmCommandSuccess(Dictionary<String, String> recParam)
        {
            return recParam.TryGetValue("success", out string successResult) && bool.Parse(successResult);
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(KafkaMessage value)
        {
            
            if (topicContents.ContainsKey(value.Topic))
            {
                //log.LogDebug(GetType(),$"adding message to topic {value.Topic}, {value.Value}");
                topicContents[value.Topic].Enqueue(value);
            }
            else
            {
                topicContents[value.Topic] = new ConcurrentQueue<KafkaMessage>(new ConcurrentBag<KafkaMessage>() { value });
            }
        }

        public IDisposable Subscribe(IObserver<AvailabilityNotification> observer)
        {
           
            if (!availabilityNotificationObservers.Contains(observer))
                availabilityNotificationObservers.Add(observer);
            return new AvailabilityNotificationUnsubscriber(availabilityNotificationObservers, observer);
        }
    }
}
