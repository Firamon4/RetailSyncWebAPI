using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RetailSyncWeb.Data;
using RetailSyncWeb.Entities;
using RetailSyncWeb.Models; // Додано для SyncStatus та PackageTypes
using RetailSyncWeb.Models.DTO;

namespace RetailSyncWeb.Services
{
    public class PackageProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PackageProcessor> _logger;
        private readonly SyncStatusService _statusService;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public PackageProcessor(IServiceProvider serviceProvider, ILogger<PackageProcessor> logger, SyncStatusService statusService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _statusService = statusService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PackageProcessor started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool processedAny = await ProcessQueue();

                    // Якщо черга була порожня, чекаємо довше (2 сек), 
                    // якщо ні - чекаємо менше (100 мс), щоб швидше розгребти завал.
                    int delay = processedAny ? 100 : 2000;
                    await Task.Delay(delay, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Критична помилка циклу обробки");
                    await Task.Delay(5000, stoppingToken); // Пауза при аварії
                }
            }
        }

        private async Task<bool> ProcessQueue()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // --- КРОК 1: Транзакційне захоплення пакетів (Fix Race Condition) ---
            // Ми відразу позначаємо їх як "Processing", щоб інші потоки їх не взяли.
            using var transaction = await db.Database.BeginTransactionAsync();

            var packagesToProcess = await db.SyncPackages
                .Where(p => p.Status == SyncStatus.New)
                .OrderBy(p => p.Id)
                .Take(50)
                .ToListAsync();

            if (!packagesToProcess.Any()) return false;

            foreach (var pkg in packagesToProcess)
            {
                pkg.Status = SyncStatus.Processing;
            }
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            // -------------------------------------------------------------------

            // --- КРОК 2: Обробка (вже поза транзакцією БД, щоб не блокувати таблицю надовго) ---
            foreach (var pkg in packagesToProcess)
            {
                _statusService.UpdateState($"Обробка пакету ID {pkg.Id}...", pkg.DataType);

                try
                {
                    // Використовуємо константи замість магічних рядків
                    switch (pkg.DataType)
                    {
                        case PackageTypes.Product: await ProcessProducts(db, pkg.Payload); break;
                        case PackageTypes.Price: await ProcessPrices(db, pkg.Payload); break;
                        case PackageTypes.Remain: await ProcessStocks(db, pkg.Payload); break;
                        case PackageTypes.Worker:
                        case PackageTypes.Users: await ProcessWorkers(db, pkg.Payload); break;
                        case PackageTypes.Shop: await ProcessStores(db, pkg.Payload); break;
                        case PackageTypes.Counterparty: await ProcessCounterparties(db, pkg.Payload); break;
                        case PackageTypes.Specification: await ProcessSpecifications(db, pkg.Payload); break;
                        case PackageTypes.Order: await ProcessInternalOrders(db, pkg.Payload); break;
                        case PackageTypes.ReturnAndComing: await ProcessTransfers(db, pkg.Payload); break;
                        default:
                            throw new Exception($"Невідомий тип даних: {pkg.DataType}");
                    }

                    // Успішне завершення
                    pkg.Status = SyncStatus.Completed;
                    pkg.ProcessedAtUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    // Fix Data Loss: Записуємо помилку, але не видаляємо пакет
                    _logger.LogError(ex, $"Помилка в пакеті {pkg.Id}");
                    _statusService.UpdateState($"❌ Помилка в пакеті {pkg.Id}", pkg.DataType);

                    pkg.Status = SyncStatus.Error;
                    pkg.ErrorMessage = ex.Message + (ex.InnerException != null ? $" | {ex.InnerException.Message}" : "");
                    pkg.ProcessedAtUtc = DateTime.UtcNow;
                }

                // Зберігаємо стан кожного пакету окремо (або групами, але тут окремо безпечніше)
                // Важливо: перехоплюємо помилку збереження самого статусу
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, $"Не вдалося зберегти статус пакету {pkg.Id}");
                }
            }

            _statusService.UpdateState("Очікування нових даних...", "-");
            return true;
        }

        // --- Методи обробки залишилися майже без змін, лише додано перевірки ---

        private async Task ProcessProducts(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<ProductDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var item in items)
            {
                if (!Guid.TryParse(item.Ref, out var id)) continue;
                var entity = await db.Products.FindAsync(id);
                if (entity == null) { entity = new Product { Id = id }; db.Products.Add(entity); }

                entity.Name = item.Name;
                entity.Code = item.Code;
                entity.Articul = item.Articul;
                entity.Barcode = item.Barcode;
                entity.IsFolder = item.IsFolder;
                entity.IsDeleted = item.IsDeleted;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task ProcessPrices(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<PriceDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var item in items)
            {
                if (!Guid.TryParse(item.ProductRef, out var pId)) continue;
                if (string.IsNullOrEmpty(item.PriceTypeRef) || !Guid.TryParse(item.PriceTypeRef, out var tId)) continue;

                var entity = await db.Prices.FindAsync(pId, tId);
                if (entity == null) { entity = new Price { ProductId = pId, PriceTypeId = tId }; db.Prices.Add(entity); }

                entity.Value = item.Price;
                entity.Currency = item.Currency;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task ProcessStocks(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<StockDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var item in items)
            {
                if (!Guid.TryParse(item.ProductUid, out var pId)) continue;
                if (!Guid.TryParse(item.Subdivision, out var wId)) continue;

                var entity = await db.Stocks.FindAsync(pId, wId);
                if (entity == null) { entity = new Stock { ProductId = pId, WarehouseId = wId }; db.Stocks.Add(entity); }

                entity.Quantity = item.Quantity;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task ProcessWorkers(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<WorkerDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var item in items)
            {
                if (!Guid.TryParse(item.Ref, out var id)) continue;
                var entity = await db.Workers.FindAsync(id);
                if (entity == null) { entity = new Worker { Id = id }; db.Workers.Add(entity); }

                entity.Name = item.WorkerName;
                entity.Position = item.PositionName;
                entity.IsActive = item.IsActual;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task ProcessCounterparties(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<CounterpartyDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var item in items)
            {
                if (!Guid.TryParse(item.Ref, out var id)) continue;
                var entity = await db.Counterparties.FindAsync(id);
                if (entity == null) { entity = new Counterparty { Id = id }; db.Counterparties.Add(entity); }

                entity.Name = item.Name;
                entity.Code = item.Code;
                entity.TaxId = item.TaxId;
                entity.IsDeleted = item.IsDeleted;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task ProcessStores(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<StoreDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var item in items)
            {
                if (!Guid.TryParse(item.Ref, out var id)) continue;
                var entity = await db.Stores.FindAsync(id);
                if (entity == null) { entity = new Store { Id = id }; db.Stores.Add(entity); }

                entity.Name = item.Name;
                entity.ShopNumber = item.ShopNumber;
                entity.PriceTypeGuid = item.PriceType;
                entity.IsDeleted = item.IsDeleted;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task ProcessSpecifications(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<SpecificationDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var dto in items)
            {
                if (!Guid.TryParse(dto.Ref, out var id)) continue;

                var entity = await db.Specifications.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) { entity = new Specification { Id = id }; db.Specifications.Add(entity); }

                entity.Number = dto.Number;
                entity.Date = dto.Date;
                entity.IsApproved = dto.IsApproved;
                entity.IsDeleted = dto.IsDeleted;
                if (Guid.TryParse(dto.CounterpartyRef, out var cId)) entity.CounterpartyId = cId;
                entity.UpdatedAt = DateTime.UtcNow;

                db.SpecificationItems.RemoveRange(entity.Items);
                foreach (var row in dto.Items)
                {
                    if (!Guid.TryParse(row.ProductRef, out var pId)) continue;
                    db.SpecificationItems.Add(new SpecificationItem { SpecificationId = id, ProductId = pId, Price = row.Price, Unit = row.Unit });
                }
            }
        }

        private async Task ProcessInternalOrders(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<InternalOrderDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var dto in items)
            {
                if (!Guid.TryParse(dto.Ref, out var id)) continue;

                var entity = await db.InternalOrders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) { entity = new InternalOrder { Id = id }; db.InternalOrders.Add(entity); }

                entity.Number = dto.Number;
                entity.Date = dto.Date;
                entity.IsApproved = dto.IsApproved;
                entity.IsDeleted = dto.IsDeleted;
                if (Guid.TryParse(dto.CounterpartyUid, out var cId)) entity.CounterpartyId = cId;
                entity.UpdatedAt = DateTime.UtcNow;

                db.InternalOrderItems.RemoveRange(entity.Items);
                foreach (var row in dto.Items)
                {
                    if (!Guid.TryParse(row.ProductRef, out var pId)) continue;
                    db.InternalOrderItems.Add(new InternalOrderItem { OrderId = id, ProductId = pId, Price = row.Price, Count = row.Count, CountFact = row.CountFact });
                }
            }
        }

        private async Task ProcessTransfers(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<TransferDto>>(json, _jsonOptions);
            if (items == null) return;

            foreach (var dto in items)
            {
                if (!Guid.TryParse(dto.Ref, out var id)) continue;

                var entity = await db.Transfers.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null) { entity = new Transfer { Id = id }; db.Transfers.Add(entity); }

                entity.Number = dto.Number;
                entity.DocType = dto.DocType;
                entity.Date = dto.Date;
                entity.IsApproved = dto.IsApproved;
                entity.IsDeleted = dto.IsDeleted;
                if (Guid.TryParse(dto.SenderUid, out var sId)) entity.SenderId = sId;
                if (Guid.TryParse(dto.RecipientUid, out var rId)) entity.RecipientId = rId;
                entity.UpdatedAt = DateTime.UtcNow;

                db.TransferItems.RemoveRange(entity.Items);
                foreach (var row in dto.Items)
                {
                    if (!Guid.TryParse(row.ProductRef, out var pId)) continue;
                    db.TransferItems.Add(new TransferItem { TransferId = id, ProductId = pId, Price = row.Price, Count = row.Count, CountReceived = row.CountReceived, CountAccepted = row.CountAccepted });
                }
            }
        }
    }
}