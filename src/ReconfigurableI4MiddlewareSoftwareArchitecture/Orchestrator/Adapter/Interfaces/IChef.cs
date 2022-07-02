using System;
using System.Threading.Tasks;
using Orchestrator.Adapter.RecipeInterpretation;
using Orchestrator.DomainObjects;

namespace Orchestrator.Adapter
{
    /// <summary>
    /// Responsible for the production of a single product
    /// </summary>
    public interface IChef : IObservable<AvailabilityNotification>
    {
        /// <summary>
        /// Responsible for producing a product based on the defined recipe. While executing the individual steps,
        /// the product is updated to enable future traceability.
        /// </summary>
        /// <param name="recipe">The recipe for the product</param>
        /// <param name="product">The product to update when steps are finished or the product is finished</param>
        /// <returns></returns>
        public Task ExecuteRecipe(Recipe recipe, Product product);
        public bool isCurrentlyExecutingRecipe();
        public void SetStopExecution(bool value);
    }
}