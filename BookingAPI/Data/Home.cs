namespace BookingAPI.Data;

public class Home 
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public List<DateOnly> AvailableSlots { get; set; } = new List<DateOnly>();
}