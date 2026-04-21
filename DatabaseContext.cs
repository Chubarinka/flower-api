using Microsoft.EntityFrameworkCore;
using FlowerApi.Models;

public class DatabaseContext : DbContext
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Favorite> Favorites { get; set; } = null!;

    public DatabaseContext()
    {
        try
        {
            Console.WriteLine("=== DatabaseContext Constructor ===");
            var connectionString = GetConnectionString();
            Console.WriteLine($"Connection string length: {connectionString?.Length ?? 0}");
            
            // Не делаем EnsureCreated здесь, сделаем в OnConfiguring
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in constructor: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    private string GetConnectionString()
    {
        // Сначала пробуем взять из переменной окружения
        var connString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
        
        if (!string.IsNullOrEmpty(connString))
        {
            Console.WriteLine("Using CONNECTION_STRING from environment");
            return connString;
        }
        
        // Если нет - используем локальную
        Console.WriteLine("Using local connection string");
        return "Host=localhost;Port=5432;Database=flowerdb;Username=postgres;Password=Roza2015";
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        try
        {
            Console.WriteLine("=== OnConfiguring called ===");
            
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = GetConnectionString();
                Console.WriteLine($"Attempting to connect to database...");
                
                optionsBuilder.UseNpgsql(connectionString);
                
                // Проверяем подключение
                Console.WriteLine("Database configured successfully");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR in OnConfiguring: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw; // Пробрасываем исключение дальше
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
            entity.HasIndex(e => e.ProductId).IsUnique();
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        Console.WriteLine("=== Model configured successfully ===");
    }
}