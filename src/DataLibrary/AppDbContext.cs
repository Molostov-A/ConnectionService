using DataLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace DataLibrary;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<IpAddress> IpAddresses { get; set; }

    public DbSet<Connection> Connections { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Используем Fluent API для создания индексов
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Индекс для таблицы ip_addresses по адресу
        modelBuilder.Entity<IpAddress>()
            .HasIndex(ip => ip.Address)
            .IsUnique();

        // Указываем составной ключ для таблицы Connection
        modelBuilder.Entity<Connection>()
            .HasKey(c => new { c.UserId, c.IpAddressId, c.ConnectedAt });

        // Индексы для таблицы connections по user_id и ip_address_id
        modelBuilder.Entity<Connection>()
            .HasIndex(c => c.UserId);

        modelBuilder.Entity<Connection>()
            .HasIndex(c => c.IpAddressId);
    }
}
