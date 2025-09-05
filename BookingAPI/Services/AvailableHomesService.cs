using BookingAPI.Data;
using BookingAPI.Repositories;

namespace BookingAPI.Services;

public class AvailableHomesService : IAvailableHomesService
{
    private readonly ILogger<AvailableHomesService> _logger;
    private readonly IHomesRepository _homesRepository;

    public AvailableHomesService(ILogger<AvailableHomesService> logger, IHomesRepository homesRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(homesRepository);
        
        _logger = logger;
        _homesRepository = homesRepository;
    }

    public async Task<Home> AddAsync(Home home)
    {
        ArgumentNullException.ThrowIfNull(home);
        
        await _homesRepository.AddAsync(home);
        
        return home; 
    }
    
    public ValueTask<IReadOnlyCollection<Home>> GetByDateRangeAsync(DateOnly from, DateOnly to)
    {
        if (from == default)
        {
            throw new ArgumentException("Start date must be provided", nameof(from));
        }

        if (to == default)
        {
            throw new ArgumentException("End date must be provided", nameof(to));
        }
        
        if (from > to)
        {
            throw new ArgumentException("Start date must be before or equal to end date", nameof(from));
        }

        _logger.LogInformation("Getting available homes from {from} to {to}", from, to);
        
        return _homesRepository.GetAvailableHomesAsync(from, to);
    } 
}