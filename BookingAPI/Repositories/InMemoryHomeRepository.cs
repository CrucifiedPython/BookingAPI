using System.Collections.Concurrent;
using BookingAPI.Data;

namespace BookingAPI.Repositories;

public class InMemoryHomeRepository : IHomesRepository
{
    private long _nextId;
    private readonly Dictionary<long, Home> _homes = new();
    private readonly ConcurrentDictionary<DateOnly, HashSet<long>> _homesByDate = new();
    
    public void Clear()
    {
        _homes.Clear();
        _homesByDate.Clear();
    }

    public Task AddAsync(Home home)
    {
        home.Id = Interlocked.Increment(ref _nextId);
        _homes[home.Id] = home;
        IndexHome(home);
        
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<Home> homes)
    {
        foreach (var home in homes)
        {
            home.Id = Interlocked.Increment(ref _nextId);
            _homes[home.Id] = home;
            IndexHome(home);
        }
        return Task.CompletedTask;
    }

    public ValueTask<IReadOnlyCollection<Home>> GetAvailableHomesAsync(DateOnly startDate, DateOnly endDate)
    {
        var requiredDates = Enumerable
            .Range(0, endDate.DayNumber - startDate.DayNumber + 1)
            .Select(startDate.AddDays)
            .ToList();

        if (!_homesByDate.TryGetValue(requiredDates[0], out var candidates))
        {
            return ValueTask.FromResult<IReadOnlyCollection<Home>>(Array.Empty<Home>());
        }
        
        var result = new HashSet<long>(candidates);
        
        foreach (var date in requiredDates.Skip(1))
        {
            if (!_homesByDate.TryGetValue(date, out var set))
            {
                return ValueTask.FromResult<IReadOnlyCollection<Home>>(Array.Empty<Home>());
            }
            
            result.IntersectWith(set);

            if (result.Count == 0)
            {
                return ValueTask.FromResult<IReadOnlyCollection<Home>>(Array.Empty<Home>());
            }
        }
        
        IReadOnlyCollection<Home> homes = result.Select(id => _homes[id]).ToList();
        return ValueTask.FromResult(homes);
    }

    private void IndexHome(Home home)
    {
        foreach (var slot in home.AvailableSlots)
        {
            var set = _homesByDate.GetOrAdd(slot, _ => []);
            lock (set)
            {
                set.Add(home.Id);
            }
        }
    }
}