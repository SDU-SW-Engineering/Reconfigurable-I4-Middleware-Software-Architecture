using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using I4ToolchainDotnetCore.Logging;
using Newtonsoft.Json.Linq;

namespace GenericAAS.Controller
{
    public class StatusUpdateHandler : IStatusUpdateHandler
    {
        
        private Dictionary<string, CancellationTokenSource> updateSources;
        private readonly II4Logger _log;
        private int DEFAULT_HERZ = 1;

        public StatusUpdateHandler(II4Logger log)
        {
            this._log = log;
            updateSources = new Dictionary<string, CancellationTokenSource>();
            
        }
        public void AddDefaultUpdateSource(string sourceName, Func<JObject> statusFunction)
        {
            StartNewUpdater(sourceName, statusFunction, DEFAULT_HERZ);
        }

        public void AddCustomUpdateSource(string sourceName, Func<JObject> statusFunction, int herz)
        {
            StartNewUpdater(sourceName, statusFunction, herz);
        }

        public void RemoveUpdateSource(string sourceName)
        {
            if(updateSources.TryGetValue(sourceName, out CancellationTokenSource cts))
            {
                cts.Cancel();
            }
            else
            {
                _log.LogError(GetType(), "Could not remove update source for {sourcename}", sourceName);
            }
        }

        private void StartNewUpdater(string sourceName, Func<JObject> statusFunction, int herz)
        {
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                cts.Cancel();
            };
            Task update = new Task(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    statusFunction();
                    await Task.Delay(1000 / herz); 
                }
            }, cts.Token);
            update.Start();
            updateSources.Add(sourceName, cts);
        }
    }
}