using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Shipment> Shipments => Set<Shipment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shipment>().HasData(
            new Shipment { Id = 1, Origin = "La Paz", Destination = "Cochabamba", Status = ShipmentStatus.Active },
            new Shipment { Id = 2, Origin = "Santa Cruz", Destination = "La Paz", Status = ShipmentStatus.Completed },
            new Shipment { Id = 3, Origin = "Tarija", Destination = "Oruro", Status = ShipmentStatus.Active }
        );
    }
}
