using Orchestrator.Adapter.RecipeInterpretation;

namespace Orchestrator.DomainObjects
{
    public struct ProductionRequest
    {
        public Recipe Recipe { get; set; }
        public string OrderId { get; set; }
    }
}