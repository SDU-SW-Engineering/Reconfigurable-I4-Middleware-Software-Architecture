using Orchestrator.Adapter.RecipeInterpretation;
using System;
using System.Collections.Generic;
using System.Text;
using Orchestrator.DomainObjects;

namespace Orchestrator.Adapter
{
    public interface ICoordinator : IObserver<AvailabilityNotification>
    {
        public Order InitializeOrder(string orderId, string recipeName, int amount = 1);
        public void StopRecipeExecution();
        public void VerifyCapabilitiesReady(string orderId);
        public Order InitializeOrderWithRecipe(string orderId, Recipe recipe, int amount = 1);

    }
}
