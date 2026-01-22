namespace RetailSyncWeb.Entities
{
    public class Stock
    {
        public Guid ProductId     { get; set; }
        public Guid WarehouseId   { get; set; }
        public decimal Quantity   { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}