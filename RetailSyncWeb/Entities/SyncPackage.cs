using System.ComponentModel.DataAnnotations;
using RetailSyncWeb.Models;

namespace RetailSyncWeb.Entities
{
    public class SyncPackage
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string Source { get; set; } = "";

        [MaxLength(50)]
        public string Target { get; set; } = "";

        [MaxLength(50)]
        public string DataType { get; set; } = "";

        public string Payload { get; set; } = "";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public SyncStatus Status { get; set; } = SyncStatus.New;

        public DateTime? ProcessedAtUtc { get; set; }

        public string? ErrorMessage { get; set; }
    }
}