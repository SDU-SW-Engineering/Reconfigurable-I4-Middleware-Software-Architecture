namespace Orchestrator.Adapter.Interfaces
{
    public interface IChefFactory
    {
        /// <summary>
        /// Responsible for creating a new IChef implementation
        /// </summary>
        /// <param name="chefId"></param>
        /// <returns></returns>
        public IChef GetNewChef(string chefId);
    }
}