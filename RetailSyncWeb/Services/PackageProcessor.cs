using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RetailSyncWeb.Data;
using RetailSyncWeb.Entities;
using RetailSyncWeb.Models.DTO;

namespace RetailSyncWeb.Services
{
    public class PackageProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PackageProcessor> _logger;

        public PackageProcessor(IServiceProvider serviceProvider, ILogger<PackageProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessQueue();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка при обробці пакетів");
                }
                await Task.Delay(5000, stoppingToken); // Пауза 5 сек
            }
        }

        private async Task ProcessQueue()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Беремо перші 50 пакетів
            var packages = await db.SyncPackages.OrderBy(p => p.Id).Take(50).ToListAsync();

            foreach (var pkg in packages)
            {
                switch (pkg.DataType)
                {
                    case "Product": await ProcessProducts(db, pkg.Payload); break;
                    case "Price": await ProcessPrices(db, pkg.Payload); break;
                    case "Remain": await ProcessStocks(db, pkg.Payload); break;
                    case "Worker": await ProcessWorkers(db, pkg.Payload); break;
                }
                db.SyncPackages.Remove(pkg); // Видаляємо після обробки
            }

            if (packages.Any()) await db.SaveChangesAsync();
        }

        // --- Методи розбору JSON ---

        private async Task ProcessProducts(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<ProductDto>>(json);
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
            var items = JsonSerializer.Deserialize<List<PriceDto>>(json);
            if (items == null) return;

            foreach (var item in items)
            {
                if (!Guid.TryParse(item.ProductRef, out var pId)) continue;
                if (!Guid.TryParse(item.PriceTypeRef, out var tId)) continue;

                var entity = await db.Prices.FindAsync(pId, tId);
                if (entity == null) { entity = new Price { ProductId = pId, PriceTypeId = tId }; db.Prices.Add(entity); }

                entity.Value = item.Price;
                entity.Currency = item.Currency;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private async Task ProcessStocks(AppDbContext db, string json)
        {
            var items = JsonSerializer.Deserialize<List<StockDto>>(json);
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
            var items = JsonSerializer.Deserialize<List<WorkerDto>>(json);
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
    }
}