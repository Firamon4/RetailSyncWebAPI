using System.ComponentModel.DataAnnotations;
namespace RetailSyncWeb.Entities
{
    public class Counterparty
    {
        [Key] public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string TaxId { get; set; } = "";
        public bool IsDeleted { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}