using Microsoft.EntityFrameworkCore;
public class LogiTrackContext : DbContext
{
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=logitrack.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DatePlaced).IsRequired();

        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.ItemId).IsRequired();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ItemId });
            entity.Property(e => e.OrderId).IsRequired();
            entity.Property(e => e.ItemId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();

            entity.HasOne(d => d.Order)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Item)
                .WithMany()
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}