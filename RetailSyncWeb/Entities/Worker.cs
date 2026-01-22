using System.ComponentModel.DataAnnotations;

namespace RetailSyncWeb.Entities
{
    public class Worker
    {
        [Key]
        public Guid Id            { get; set; }
        public string Name        { get; set; } = "";
        public string LoginCode   { get; set; } = "";
        public Guid? StoreId      { get; set; }
        public string Position    { get; set; } = "";
        public bool IsActive      { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}