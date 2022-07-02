using System;
using System.Threading.Tasks;
using Orchestrator.Adapter.RecipeInterpretation;
using Orchestrator.DomainObjects;

namespace Orchestrator.Adapter
{
    public interface IChef : IObservable<AvailabilityNotification>
    {
        public Task ExecuteRecipe(Recipe recipe, Product product);
        public bool isCurrentlyExecutingRecipe();
        public void SetStopExecution(bool value);
    }
}