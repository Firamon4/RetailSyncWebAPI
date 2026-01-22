using System.ComponentModel.DataAnnotations;

namespace RetailSyncWeb.Entities
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } 

        public string Name        { get; set; } = "";
        public string Code        { get; set; } = "";
        public string Articul     { get; set; } = "";
        public string Barcode     { get; set; } = "";

        public bool IsFolder      { get; set; }
        public bool IsDeleted     { get; set; }
        public Guid? ParentId     { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}