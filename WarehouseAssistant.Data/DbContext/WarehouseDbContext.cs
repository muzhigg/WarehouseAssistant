using Microsoft.EntityFrameworkCore;
using WarehouseAssistant.Data.Models;

namespace WarehouseAssistant.Data.DbContexts;

public class WarehouseDbContext : DbContext
{
    public DbSet<Product>?           Products           { get; set; }
    public DbSet<MarketingMaterial>? MarketingMaterials { get; set; }

    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Article);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Barcode);
            entity.Property(e => e.QuantityPerBox);
            entity.Property(e => e.QuantityPerShelf);
        });

        modelBuilder.Entity<MarketingMaterial>(entity =>
        {
            entity.HasKey(e => e.Article);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.PackArticles).HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
        });
    }
}