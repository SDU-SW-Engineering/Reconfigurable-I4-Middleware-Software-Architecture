using Orchestrator.Adapter.RecipeInterpretation;
using System;
using System.Collections.Generic;
using System.Text;
using Orchestrator.DomainObjects;

namespace Orchestrator.Adapter
{
    /// <summary>
    /// Responsible for managing the execution of an Order, i.e. managing the individual chefs and coordinating the
    /// production of all products required to fulfil the order.
    /// </summary>
    public interface ICoordinator : IObserver<AvailabilityNotification>
    {
        /// <summary>
        /// Responsible for initializing an order only with a recipe ID, i.e. queuing it or initializing the production immediately
        /// </summary>
        /// <param name="orderId">Identification of the order</param>
        /// <param name="recipeName">if of the recipe for the products in the order</param>
        /// <param name="amount">amount of products to be produced</param>
        /// <returns></returns>
        public Order InitializeOrder(string orderId, string recipeName, int amount = 1);
        /// <summary>
        /// Responsible for stopping all productions
        /// </summary>
        public void StopRecipeExecution();
        /// <summary>
        /// Responsible for handling the message that all capabilities required to start production of
        /// an order are ready.
        /// </summary>
        /// <param name="orderId"></param>
        public void VerifyCapabilitiesReady(string orderId);
        /// <summary>
        /// Responsible for starting an order, if the recipe was sent in the order (and not only the ID of the recipe)
        /// </summary>
        /// <param name="orderId">Identification of the order</param>
        /// <param name="recipe">The entire recipe used to produce the products in the order</param>
        /// <param name="amount">amount of products to be produced</param>
        /// <returns></returns>
        public Order InitializeOrderWithRecipe(string orderId, Recipe recipe, int amount = 1);

    }
}
