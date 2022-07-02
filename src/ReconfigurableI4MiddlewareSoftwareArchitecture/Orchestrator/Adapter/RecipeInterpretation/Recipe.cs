using System;
using System.Collections.Generic;
using System.Text;

namespace Orchestrator.Adapter.RecipeInterpretation
{
    public class Recipe : IRecipe
    {
        private List<Step> steps1 = new List<Step>();

        public String recipeName { get; set; }
        public List<Step> steps { get => steps1; set => steps1 = value; }

        public override string ToString()
        {
            return $"Recipename: {recipeName}, amount of steps: {steps.Count}";
        }
    }
}
