using System;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;

namespace GenericAAS.Controller
{
    /// <summary>
    /// Not Used
    /// </summary>
    public interface IStatusUpdateHandler
    {
        public void AddDefaultUpdateSource(string sourceName, Func<JObject> statusFunction);
        public void AddCustomUpdateSource(string sourceName, Func<JObject> statusFunction, int freqPerSecond);
        public void RemoveUpdateSource(string sourceName);
    }
}