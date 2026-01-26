using System.ComponentModel.DataAnnotations;

namespace RetailSyncWeb.Entities
{
    // === СПЕЦИФІКАЦІЯ ===
    public class Specification
    {
        [Key] public Guid Id { get; set; }
        public string Number { get; set; } = "";
        public DateTime Date { get; set; }
        public Guid? CounterpartyId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<SpecificationItem> Items { get; set; } = new();
    }

    public class SpecificationItem
    {
        [Key] public int Id { get; set; }
        public Guid SpecificationId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; } = "";
    }

    // === ВНУТРІШНЄ ЗАМОВЛЕННЯ ===
    public class InternalOrder
    {
        [Key] public Guid Id { get; set; }
        public string Number { get; set; } = "";
        public DateTime Date { get; set; }
        public Guid? CounterpartyId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<InternalOrderItem> Items { get; set; } = new();
    }

    public class InternalOrderItem
    {
        [Key] public int Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public decimal Count { get; set; }
        public decimal CountFact { get; set; }
    }

    // === ПЕРЕМІЩЕННЯ (Прихід/Повернення) ===
    public class Transfer
    {
        [Key] public Guid Id { get; set; }
        public string Number { get; set; } = "";
        public string DocType { get; set; } = ""; // "Coming" або "Return"
        public DateTime Date { get; set; }
        public Guid? SenderId { get; set; }
        public Guid? RecipientId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public List<TransferItem> Items { get; set; } = new();
    }

    public class TransferItem
    {
        [Key] public int Id { get; set; }
        public Guid TransferId { get; set; }
        public Guid ProductId { get; set; }
        public decimal Price { get; set; }
        public decimal Count { get; set; }
        public decimal CountReceived { get; set; }
        public decimal CountAccepted { get; set; }
    }
}