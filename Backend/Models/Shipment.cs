namespace Backend.Models;

public enum ShipmentStatus { Active = 0, Completed = 1 }

public class Shipment
{
    public int Id { get; set; } 
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public ShipmentStatus Status { get; set; }
}
