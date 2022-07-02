using I4ToolchainDotnetCore.Logging;
using Orchestrator.Adapter.RecipeInterpretation;
using Orchestrator.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter
{
    public class Cookbook : ICookbook
    {
        private readonly II4Logger log;
        Dictionary<String, Recipe> recipes;

        public Cookbook(II4Logger log)
        {
            recipes = new Dictionary<String, Recipe>();
            this.log = log;
        }

        public void AddRecipe(String recipeName, Recipe recipe)
        {
            recipes.Add(recipeName, recipe);
        }

        public Recipe GetRecipe(String recipeName)
        {
            Recipe recipe = null;
            if (recipes.TryGetValue(recipeName, out recipe))
            {
                return recipe;
            }
            else
            {
                log.LogError(GetType(), "The recipe {recipeName} could not be found in the cookbook");
                throw new RecipeNotFoundException($"The Recipe {recipeName} could not be found");
            }
        }

        public Dictionary<String, Recipe> GetAllRecipes()
        {
            return recipes;
        }
    }
}
