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
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="recipeName"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Order InitializeOrder(string orderId, string recipeName, int amount = 1);
        public void StopRecipeExecution();
        public void VerifyCapabilitiesReady(string orderId);
        public Order InitializeOrderWithRecipe(string orderId, Recipe recipe, int amount = 1);

    }
}
