using System.Diagnostics;
using BookingAPI.Data;
using BookingAPI.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace BookingAPI.PerformanceTests;

public class PerformanceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public PerformanceTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));
        
        _factory = factory;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task Benchmark_GetAvailableHomesAsync_100kHomes()
    {
        const int totalHomes = 100000;
        const int availableDays = 30;
        
        var startDate = new DateOnly(2025, 9, 1);
        var endDate = startDate.AddDays(29);

        var homes = Enumerable.Range(1, totalHomes)
            .Select(i => new Home
            {
                Name = $"Home {i}",
                AvailableSlots = Enumerable.Range(0, availableDays)
                    .Select(offset => startDate.AddDays(offset))
                    .ToList()
            })
            .ToList();

        using var scope = _factory.Services.CreateScope();
        var repo = (InMemoryHomeRepository)scope.ServiceProvider.GetRequiredService<IHomesRepository>();
        repo.Clear();
        await repo.AddRangeAsync(homes);

        var stopwatch = Stopwatch.StartNew();

        var result = await repo.GetAvailableHomesAsync(startDate, endDate);

        stopwatch.Stop();

        _testOutputHelper.WriteLine($"Retrieved {result.Count} homes in {stopwatch.ElapsedMilliseconds} ms");

        Assert.Equal(totalHomes, result.Count);
    }
}