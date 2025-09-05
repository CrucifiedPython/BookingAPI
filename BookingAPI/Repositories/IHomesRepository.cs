using BookingAPI.Data;
using BookingAPI.Services;

namespace BookingAPI.Repositories;

public interface IHomesRepository
{
    public Task AddAsync(Home home);
    public Task AddRangeAsync(IEnumerable<Home> homes);
    public ValueTask<IReadOnlyCollection<Home>> GetAvailableHomesAsync(DateOnly startDate, DateOnly endDate);
}