namespace Orchestrator.Adapter.Interfaces
{
    public interface IChefFactory
    {
        public IChef GetNewChef(string chefId);
    }
}