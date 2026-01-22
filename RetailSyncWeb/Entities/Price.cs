namespace RetailSyncWeb.Entities
{
    public class Price
    {
        public Guid ProductId     { get; set; }
        public Guid PriceTypeId   { get; set; }
        
        public decimal Value      { get; set; }
        public string Currency    { get; set; } = "UAH";
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}