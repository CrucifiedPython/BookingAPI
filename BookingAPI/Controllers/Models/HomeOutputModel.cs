namespace BookingAPI.Controllers.Models;

public class HomeOutputModel
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public List<DateOnly> AvailableSlots { get; set; } = new List<DateOnly>();
}