using Microsoft.EntityFrameworkCore;
using RetailSyncWeb.Entities;

namespace RetailSyncWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SyncPackage> SyncPackages { get; set; }

        // Довідники
        public DbSet<Product> Products { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Counterparty> Counterparties { get; set; }
        public DbSet<Store> Stores { get; set; }

        // Документи
        public DbSet<Specification> Specifications { get; set; }
        public DbSet<SpecificationItem> SpecificationItems { get; set; }
        public DbSet<InternalOrder> InternalOrders { get; set; }
        public DbSet<InternalOrderItem> InternalOrderItems { get; set; }
        public DbSet<Transfer> Transfers { get; set; }
        public DbSet<TransferItem> TransferItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Price>().HasKey(p => new { p.ProductId, p.PriceTypeId });
            modelBuilder.Entity<Stock>().HasKey(s => new { s.ProductId, s.WarehouseId });
        }
    }
}