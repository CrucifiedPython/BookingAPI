using BookingAPI.Controllers;
using BookingAPI.Data;

namespace BookingAPI.Services;

public interface IAvailableHomesService
{
    Task<Home> AddAsync(Home home);
    ValueTask<IReadOnlyCollection<Home>> GetByDateRangeAsync(DateOnly from, DateOnly to);
}