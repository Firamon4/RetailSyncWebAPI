namespace RetailSyncWeb.Services
{
    public class SyncStatusService
    {
        // Подія, на яку підпишеться Dashboard
        public event Action? OnChange;

        // Дані для відображення в Real-Time
        public string LastAction { get; private set; } = "Очікування...";
        public string LastPackageType { get; private set; } = "-";
        public int TotalProcessed { get; private set; } = 0;
        public DateTime LastUpdate { get; private set; } = DateTime.Now;

        // Метод, який викликає PackageProcessor
        public void UpdateState(string action, string packageType)
        {
            LastAction = action;
            LastPackageType = packageType;
            TotalProcessed++;
            LastUpdate = DateTime.Now;

            // Сповіщаємо всіх, хто підписався (Dashboard)
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}