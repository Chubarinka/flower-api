using Microsoft.EntityFrameworkCore;
using FlowerApi.Models;

public class DatabaseContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;

    public DatabaseContext()
    {
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Сначала пытаемся взять строку подключения из переменных окружения (для Render)
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            
            // Если переменной нет - используем локальную строку для разработки
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = "Host=localhost;Port=5432;Database=flowerdb;Username=postgres;Password=Roza2015";
            }
            
            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка для Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Composition).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Price).IsRequired().HasPrecision(10, 2);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(10);
            
            // Храним списки как JSON
            entity.Property(e => e.Flowers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new());
            
            entity.Property(e => e.Occasion)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new());
            
            entity.Property(e => e.CompositionType).HasMaxLength(20);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
        });

        // Настройка для Favorite
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.Id);
    
            entity.HasIndex(e => e.ProductId)
                .IsUnique();
    
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}