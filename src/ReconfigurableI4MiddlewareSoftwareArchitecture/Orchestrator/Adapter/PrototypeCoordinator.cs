using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchestrator;
using Orchestrator.Adapter;
using Toolchain.Kafka;
using VDS.Common.Collections.Enumerations;

public class PrototypeCoordinator{

    private KafkaClient client;
    private Database database;
    private List<string> opcuaNames;
    private List<string> canceledList;
    private HttpClient httpClient;
    private Tuple<string, string, string, string[], string, string> step;

    public PrototypeCoordinator()
    {
        database = new Database();
        client = new KafkaClient("OrchestratorService", "192.168.1.11:9092");
        canceledList = new List<string>();
        httpClient = new HttpClient();
    }
    
    public void CancelRecipyExecution(string recipyName)
    {
        canceledList.Add(recipyName);
        ResetOPCUAStates();
        client.SimpleProduce(step.Item1, new List<Message<Null, string>>()
        {
            new Message<Null, string>(){Value = "{\"cancelation\":true}"}
        });
    }

    public bool ExecuteRecipy(string recipyName, string orderId)
    {
        //orderId is static 123456789
        var savedVariables = new Dictionary<string, string>();
        var recipy = database.GetRecipy(recipyName);
        opcuaNames = recipy.Select(r => r.Item5).ToList();
        opcuaNames.AddRange(recipy.Select(r => r.Item6).ToList());
        ResetOPCUAStates();
        foreach (var step in recipy)
        {
            if (canceledList.Contains(recipyName))
            {
                resetRecipyAssets(recipyName, step, savedVariables);
                Console.WriteLine(recipyName + " has been cancel mid execution");
                return false;
            }
            this.step = step;
            
            WriteValue(step.Item5, 1);
            var message = step.Item2;
            Match m = Regex.Match(message, @"\$.*", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            while (m.Success)
            {
                string replacement = m.Value.Substring(1).Trim();
                string value = savedVariables[replacement];
                message = Regex.Replace(message, @"\$"+ replacement, value);
                m = Regex.Match(message, @"\$.*", RegexOptions.IgnorePatternWhitespace|RegexOptions.IgnoreCase);
            }
            client.SimpleProduce(step.Item1, new List<Message<Null, string>>()
            {
                new Message<Null, string>(){Value = message}
            });
            while(true){
                if (canceledList.Contains(recipyName))
                {
                    resetRecipyAssets(recipyName, step, savedVariables);
                    Console.WriteLine(recipyName+ " has been cancel mid execution");
                    return false;
                }
                var x = client.ManualConsume(step.Item1, AutoOffsetReset.Earliest);
                if (x.Message.Timestamp.UtcDateTime.ToLocalTime().AddSeconds(30) < DateTime.Now)
                {
                    continue;
                }
                var sampleString = step.Item3;
                Match mSample = Regex.Match(sampleString, @"\$.*", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
                while (mSample.Success)
                {
                    string value = savedVariables[mSample.Value.Substring(1)];
                    sampleString = Regex.Replace(sampleString, @"\$" + m.Value.Substring(1).Trim(), value);
                    mSample = Regex.Match(sampleString, @"\$.*", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
                }
                JObject json = null;
                try
                {
                    json = JObject.Parse(x.Message.Value);
                }
                catch(JsonReaderException e)
                {
                    continue;
                }
                var sampleJson = JObject.Parse(sampleString);
                if (json.Value<string>("@type") != "response") continue;
                foreach (var tuple in sampleJson.Properties().Select(p => new Tuple<string, JToken>(p.Name, p.Value)).ToList())
                {
                    if (tuple.Item2.Type == JTokenType.Null)
                    {
                        sampleJson[tuple.Item1] = json[tuple.Item1];
                    }
                }
                foreach (var tuple in sampleJson.Value<JObject>("response").Properties().Select(p => new Tuple<string, JToken>(p.Name, p.Value)).ToList())
                {
                    if (tuple.Item2.Type == JTokenType.Null)
                    {
                        sampleJson.Value<JObject>("response")[tuple.Item1] = json.Value<JObject>("response")[tuple.Item1];
                    }
                }
                if (json.ToString().Equals(sampleJson.ToString()))
                {
                    if (step.Item4 != null)
                    {
                        foreach (string s in step.Item4)
                        {
                            if (json.Value<JObject>("response").ContainsKey(s))
                            {
                                if (savedVariables.ContainsKey(s)){
                                    savedVariables[s] = json.Value<JObject>("response").GetValue(s).ToString();
                                } else
                                {
                                    savedVariables.Add(s, json.Value<JObject>("response").GetValue(s).ToString());
                                }
                                    
                            }
                        }
                    }
                    Console.WriteLine("Finished!");
                    WriteValue(step.Item6, 1);
                    Thread.Sleep(500);
                    break;
                }
            }
            Console.WriteLine("Continued!");
        }
        ResetOPCUAStates(); //resetting 
        return true;
    }

    private void resetRecipyAssets(string recipyName, Tuple<string, string, string, string[], string, string> stepOfRecipy, Dictionary<string, string> savedVariables)
    {
        canceledList.Remove(recipyName);
        database.GetCancelProcedure(recipyName, database.GetRecipy(recipyName).IndexOf(stepOfRecipy)).ForEach(step =>
        {
            var message = step.Item2;
            Match m = Regex.Match(message, @"\$.*", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            while (m.Success)
            {
                string replacement = m.Value.Substring(1).Trim();
                string value = savedVariables[replacement];
                message = Regex.Replace(message, @"\$" + replacement, value);
                m = Regex.Match(message, @"\$.*", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            }
            client.SimpleProduce(step.Item1, new List<Message<Null, string>>()
            {
                new Message<Null, string>(){Value = message}
            });
        });
    }

    private void ResetOPCUAStates()
    {
        opcuaNames.ForEach(n => WriteValue(n, 0));
    }

    private void WriteValue(string key, int value)
    {

        string ConnectionString = "http://192.168.1.11:5000";
        JObject message = new JObject();
        message.Add("key", key);
        message.Add("value", value);
        httpClient.PutAsync(ConnectionString, new StringContent(message.ToString(), Encoding.UTF8, "application/json"));
    }
}