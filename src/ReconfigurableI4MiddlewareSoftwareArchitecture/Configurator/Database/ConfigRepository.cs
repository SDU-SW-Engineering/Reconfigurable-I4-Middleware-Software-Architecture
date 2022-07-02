using Configurator.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Configurator.Database
{
    public class ConfigRepository : IRepository<Recipe>
    {
        private IDatabase _db;
        public ConfigRepository(IDatabase database)
        {
            _db = database;
        }
        public Recipe create(Recipe entity)
        {
            throw new NotImplementedException();
        }

        public Recipe delete(Recipe entity)
        {
            throw new NotImplementedException();
        }

        public List<Recipe> read()
        {
            throw new NotImplementedException();
        }

        public Recipe readById(int id)
        {
            return _db.ReadRecipeById(id);
        }

        public Recipe update(Recipe entity)
        {
            throw new NotImplementedException();
        }
    }
}
