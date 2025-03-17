using ConnectionLogger.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ConnectionLogger.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<IpAddress> IpAddresses { get; set; }

    public DbSet<Connection> Connections { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IpAddress>()
            .HasIndex(ip => ip.Address)
            .IsUnique();

        modelBuilder.Entity<Connection>()
            .HasKey(c => new { c.UserId, c.IpAddressId, c.ConnectedAt });

        modelBuilder.Entity<Connection>()
            .HasIndex(c => c.UserId);

        modelBuilder.Entity<Connection>()
            .HasIndex(c => c.IpAddressId);
    }
}
