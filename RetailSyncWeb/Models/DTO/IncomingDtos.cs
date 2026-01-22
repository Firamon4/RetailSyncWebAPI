using System.Text.Json.Serialization;

namespace RetailSyncWeb.Models.DTO
{
    // === ДОВІДНИКИ ===

    public class ProductDto
    {
        public string Ref { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Articul { get; set; }
        public string Barcode { get; set; }
        public bool IsFolder { get; set; }
        public bool IsActual { get; set; }
        public bool IsDeleted { get; set; }
        public string ParentRef { get; set; }
    }

    public class CounterpartyDto
    {
        public string Ref { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string TaxId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class StoreDto 
    {
        public string Ref { get; set; }
        public string Name { get; set; }
        public string ShopNumber { get; set; }
        public bool IsDeleted { get; set; }
        public string PriceType { get; set; } 
        public string Subdivision { get; set; }
        public string SubdivisionName { get; set; }
    }

    public class WorkerDto
    {
        public string Ref { get; set; }
        public string WorkerName { get; set; }
        public string Subdivision { get; set; }
        public bool IsActual { get; set; }
        public string PositionName { get; set; }
    }

    // === ДОКУМЕНТИ ===

    public class SpecificationDto
    {
        public string Ref { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string CounterpartyRef { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }
        public string PriceType { get; set; }
        public List<SpecificationItemDto> Items { get; set; } = new();
    }

    public class SpecificationItemDto
    {
        public string ProductRef { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
    }

    public class InternalOrderDto
    {
        public string Ref { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public string CounterpartyUid { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }
        public List<InternalOrderItemDto> Items { get; set; } = new();
    }

    public class InternalOrderItemDto
    {
        public string ProductRef { get; set; }
        public decimal Price { get; set; }
        public decimal Count { get; set; }
        public decimal CountFact { get; set; }
        public string Unit { get; set; }
    }

    public class TransferDto 
    {
        public string Ref { get; set; }
        public string Number { get; set; }
        public string DocType { get; set; } 
        public DateTime Date { get; set; }
        public string SenderUid { get; set; }
        public string RecipientUid { get; set; }
        public string OrderUid { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsApproved { get; set; }
        public List<TransferItemDto> Items { get; set; } = new();
    }

    public class TransferItemDto
    {
        public string ProductRef { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal Count { get; set; }
        public decimal CountReceived { get; set; }
        public decimal CountAccepted { get; set; }
        public decimal CountInOrder { get; set; }
        public string Unit { get; set; }
    }

    // === РЕГІСТРИ ===

    public class StockDto
    {
        public string Subdivision { get; set; }
        public string SubdivisionName { get; set; }
        public string ProductUid { get; set; }
        public decimal Quantity { get; set; }
    }

    public class PriceDto
    {
        public string PriceTypeRef { get; set; }
        public string ProductRef { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
    }
}