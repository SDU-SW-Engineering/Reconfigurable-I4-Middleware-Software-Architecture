using Orchestrator.Adapter.RecipeInterpretation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter
{
    public interface ICookbook
    {
        public void AddRecipe(String recipeName, Recipe recipe);
        public Recipe GetRecipe(String recipeName);
        public Dictionary<String, Recipe> GetAllRecipes();

    }
}
