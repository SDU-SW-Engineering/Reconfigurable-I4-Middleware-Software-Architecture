using Configurator.Model;
using I4ToolchainDotnetCore.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Configurator.Database
{
    /// <summary>
    /// A quickstart guide for mongodb can be found here: https://mongodb.github.io/mongo-csharp-driver/2.13/getting_started/quick_tour/#get-a-single-document-with-a-filter
    /// </summary>
    public class MongoDatabase : IDatabase
    {
        private IMongoClient client;
        private IMongoDatabase db;
        private IMongoCollection<Recipe> collection;
        private IConfiguration _config;
        private II4Logger _log;
        public MongoDatabase(IConfiguration config, II4Logger log)
        {
            _log = log;
            _config = config;
            InitializeDatabase();
            InsertAndRetrieveTestData();
        }
        private void InitializeDatabase()
        {
            _log.LogDebug(GetType(), "Initializing database...");
            client = new MongoClient($"mongodb://{_config.GetValue<string>("MONGO_HOST")}:{_config.GetValue<string>("MONGO_PORT")}");
            db = client.GetDatabase("test");
            BsonClassMap.RegisterClassMap<Recipe>();
            collection = db.GetCollection<Recipe>("test");
        }

        private void InsertAndRetrieveTestData()
        {
            _log.LogDebug(GetType(), "Inserting test-data...");
            InsertRecipe(new Recipe() { id = new ObjectId(), name = "Recipe A", steps = new List<Step>() { new Step() { id = 1, name = "step A", action = "some action" } } });
            _log.LogDebug(GetType(), "Retrieving test-data...");
            var foundConfig = FindRecipe("Recipe A");
            _log.LogDebug(GetType(), $"found: {foundConfig}");
        }

        public void InsertRecipe(Recipe Recipe)
        {
            collection.InsertOne(Recipe);
        }

        public Recipe FindRecipe(string name)
        {
            var filter = Builders<Recipe>.Filter.Eq("name", name);
            var config = collection.Find(filter).First();
            return config;
        }

        public Recipe CreateRecipe(Recipe config)
        {
            throw new NotImplementedException();
        }

        public Recipe ReadRecipeById(int id)
        {
            throw new NotImplementedException();
        }

        public Recipe UpdateRecipe(Recipe config)
        {
            throw new NotImplementedException();
        }

        public Recipe DeleteRecipe(Recipe config)
        {
            throw new NotImplementedException();
        }
    }
}
