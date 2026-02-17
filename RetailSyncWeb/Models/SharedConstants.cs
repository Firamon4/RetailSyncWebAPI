namespace RetailSyncWeb.Models
{
    public enum SyncStatus
    {
        New = 0,
        Processing = 1,
        Completed = 2,
        Error = 3
    }

    public static class PackageTypes
    {
        public const string Product = "Product";
        public const string Price = "Price";
        public const string Remain = "Remain";
        public const string Worker = "Worker";
        public const string Users = "Users";
        public const string Shop = "Shop";
        public const string Counterparty = "Counterparty";
        public const string Specification = "Specification";
        public const string Order = "Order";
        public const string ReturnAndComing = "ReturnAndComing";
    }
}