namespace GenericAAS.DataModel
{
    public class AssetStatus
    {
        public AssetStatus(bool isConnected, bool isBusy, Step currentlyExecutingStep)
        {
            IsConnected = isConnected;
            IsBusy = isBusy;
            CurrentlyExecutingStep = currentlyExecutingStep;
        }

        public AssetStatus()
        {
        }

        public bool IsConnected { get; set; }
        public bool IsBusy { get; set; }
        public Step CurrentlyExecutingStep { get; set; }
    }
}