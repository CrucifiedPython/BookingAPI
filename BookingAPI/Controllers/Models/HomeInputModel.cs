namespace BookingAPI.Controllers.Models;

public class HomeInputModel
{
    public required string Name { get; set; }
    
    public List<DateOnly> AvailableSlots { get; set; } = new List<DateOnly>();
}