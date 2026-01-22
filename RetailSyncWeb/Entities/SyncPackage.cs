using System.ComponentModel.DataAnnotations;

namespace RetailSyncWeb.Entities
{
    public class SyncPackage
    {
        [Key]
        public int Id                { get; set; }
        public string Source         { get; set; } = ""; 
        public string Target         { get; set; } = "";
        public string DataType       { get; set; } = ""; 
        public string Payload        { get; set; } = ""; 
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}