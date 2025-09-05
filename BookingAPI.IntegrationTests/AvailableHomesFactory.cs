using BookingAPI.Data;

namespace BookingAPI.Tests;

public static class AvailableHomesFactory
{
    public static Home Create(DateOnly start, DateOnly end, string name)
    {
        var home = new Home { Name = name };
        for (var d = start; d <= end; d = d.AddDays(1))
        {
            home.AvailableSlots.Add(d);
        }
        
        return home;
    }
}