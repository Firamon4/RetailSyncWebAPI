using System.ComponentModel.DataAnnotations;
namespace RetailSyncWeb.Entities
{
    public class Store
    {
        [Key] public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string ShopNumber { get; set; } = "";
        public string PriceTypeGuid { get; set; } // Тип ціни для магазину
        public bool IsDeleted { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}