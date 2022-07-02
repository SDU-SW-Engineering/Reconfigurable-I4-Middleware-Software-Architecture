using Orchestrator.Adapter.RecipeInterpretation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter
{
    /// <summary>
    /// Responsible for storing and providing access to recipes
    /// </summary>
    public interface ICookbook
    {
        /// <summary>
        /// Responsible for adding a recipe to the list
        /// </summary>
        /// <param name="recipeName">the ID of the recipe, used for later retrieval</param>
        /// <param name="recipe">The recipe</param>
        public void AddRecipe(String recipeName, Recipe recipe);
        /// <summary>
        /// Responsible for retrieving a recipe based on the provided ID.
        /// </summary>
        /// <param name="recipeName"></param>
        /// <returns></returns>
        public Recipe GetRecipe(String recipeName);
        /// <summary>
        /// Responsible for providing all recipes in a dictionary, including their IDs
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, Recipe> GetAllRecipes();

    }
}
