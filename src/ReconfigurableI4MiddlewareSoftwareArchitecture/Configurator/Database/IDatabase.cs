using System;
using System.Collections.Generic;
using System.Text;
using Configurator.Model;

namespace Configurator.Database
{
    public interface IDatabase
    {
        Recipe CreateRecipe(Recipe config);
        Recipe ReadRecipeById(int id);
        Recipe UpdateRecipe(Recipe config);
        Recipe DeleteRecipe(Recipe config);
    }
}
