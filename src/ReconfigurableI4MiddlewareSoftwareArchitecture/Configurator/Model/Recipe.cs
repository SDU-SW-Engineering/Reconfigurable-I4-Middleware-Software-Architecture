using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator.Model
{
    public class Recipe
    {
        [BsonId]
        public  ObjectId id { get; set; }
        public string name { get; set; }
        public List<Step> steps { get; set; }

        public override string ToString()
        {
            return $"Configuration with id: {id}, name: {name} and amount of steps: {steps.Count}";
        }
    }
}
