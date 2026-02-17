namespace RetailSyncWeb.Services
{
    public class SyncStatusService
    {
        public event Action? OnChange;

        public string LastAction { get; private set; } = "Очікування...";
        public string LastPackageType { get; private set; } = "-";
        public int TotalProcessed { get; private set; } = 0;
        public DateTime LastUpdate { get; private set; } = DateTime.Now;

        public void UpdateState(string action, string packageType)
        {
            LastAction = action;
            LastPackageType = packageType;
            TotalProcessed++;
            LastUpdate = DateTime.Now;

            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}