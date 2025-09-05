using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookingAPI.Controllers.Models;
using BookingAPI.Data;
using BookingAPI.Repositories;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookingAPI.Tests;

public class HomesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HomesControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        
        _factory = factory;
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task AddAsync_WhenHomeIsValid_ReturnsOkAndPersistsHome()
    {
        using var scope = _factory.Services.CreateScope();
        var repo = (InMemoryHomeRepository)scope.ServiceProvider.GetRequiredService<IHomesRepository>();
        repo.Clear();
        
        var newHome = new HomeInputModel
        {
            Name = "Test Home",
            AvailableSlots = new List<DateOnly>
            {
                new DateOnly(2025, 9, 2),
                new DateOnly(2025, 9, 3)
            }
        };

        var url = "/api/available-homes";

        var response = await _client.PostAsJsonAsync(url, newHome);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var createdHome = await response.Content.ReadFromJsonAsync<Home>();
        
        Assert.NotNull(createdHome);
        Assert.Equal(newHome.Name, createdHome.Name);
        Assert.True(createdHome.Id > 0);
        Assert.Equal(newHome.AvailableSlots.Count, createdHome.AvailableSlots.Count);
    }
    
    [Theory]
    [InlineData("2025-13-01", "2025-12-02")] 
    [InlineData("2025-09-02", "2025-09-01")] 
    [InlineData("", "2025-09-02")]           
    [InlineData("2025-09-02", "")]           
    public async Task GetAvailableHomes_WhenDatesAreInvalid_ReturnsBadRequest(string startDate, string endDate)
    {
        var url = $"/api/available-homes?startDate={startDate}&endDate={endDate}";

        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GetAvailableHomes_WhenMatchingDateRange_ReturnsOnlyMatchingHomes()
    {
        var startDate = new DateOnly(2025, 9, 2);
        var endDate = new DateOnly(2025, 9, 5);

        var home1 = AvailableHomesFactory.Create(startDate, endDate, "Home 1");
        var home2 = AvailableHomesFactory.Create(startDate, endDate.AddDays(-1), "Home 2"); 
        var home3 = AvailableHomesFactory.Create(startDate.AddDays(1), endDate, "Home 3");

        await SeedHomes(home1, home2, home3);

        var url = $"/api/available-homes?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var homes = JsonSerializer.Deserialize<List<HomeOutputModel>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(homes);
        Assert.Single(homes); 
        Assert.Equal(home1.Name, homes[0].Name);
    }

    [Fact]
    public async Task GetAvailableHomes_WhenRequestedDatesPartiallyOverlap_ReturnsEmptyList()
    {
        var startDate = new DateOnly(2025, 9, 2);
        var endDate = new DateOnly(2025, 9, 5);

        var home1 = AvailableHomesFactory.Create(startDate, endDate.AddDays(-1), "Home 1"); 
        var home2 = AvailableHomesFactory.Create(startDate.AddDays(1), endDate, "Home 2");
        
        await SeedHomes(home1, home2);

        var url = $"/api/available-homes?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var homes = JsonSerializer.Deserialize<List<HomeOutputModel>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(homes);
        Assert.Empty(homes);
    }
    
    [Fact]
    public async Task GetAvailableHomes_WhenNoHomesAvailableInRange_ReturnsEmptyList()
    {
        var startDate = new DateOnly(2025, 9, 2);
        var endDate = new DateOnly(2025, 9, 5);

        var home1 = AvailableHomesFactory.Create(startDate.AddDays(-4), startDate.AddDays(-1), "Home 1"); 
        var home2 = AvailableHomesFactory.Create(endDate.AddDays(3), endDate.AddDays(5), "Home 2");
        
        await SeedHomes(home1, home2);

        var url = $"/api/available-homes?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
        
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var homes = JsonSerializer.Deserialize<List<HomeOutputModel>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(homes);
        Assert.Empty(homes);
    }

    [Fact]
    public async Task GetAvailableHomes_WhenSingleDayRange_ReturnsCorrectHomes()
    {
        var singleDate = new DateOnly(2025, 9, 2);

        var home1 = AvailableHomesFactory.Create(singleDate, singleDate.AddDays(2), "Home 1"); 
        var home2 = AvailableHomesFactory.Create(singleDate.AddDays(-1), singleDate, "Home 2");
        var home3 = AvailableHomesFactory.Create(singleDate.AddDays(3), singleDate.AddDays(4), "Home 3");
        
        await SeedHomes(home1, home2, home3);

        var url = $"/api/available-homes?startDate={singleDate:yyyy-MM-dd}&endDate={singleDate:yyyy-MM-dd}";
        
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var homes = JsonSerializer.Deserialize<List<HomeOutputModel>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(homes);
        Assert.Equal(2, homes.Count);
        
        var homeNames = homes.Select(h => h.Name).ToList();
        Assert.Contains(home1.Name, homeNames);
        Assert.Contains(home2.Name, homeNames);
    }
    
    [Fact]
    public async Task GetAvailableHomes_WhenDatasetIsLarge_ReturnsAllMatchingHomes()
    {
        const int totalHomes = 10000;
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
        var repo = (InMemoryHomeRepository)_factory.Services.GetRequiredService<IHomesRepository>();
        repo.Clear();
        await repo.AddRangeAsync(homes);

        var url = $"/api/available-homes?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";

        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var returnedHomes = JsonSerializer.Deserialize<List<HomeOutputModel>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(returnedHomes);
        Assert.Equal(totalHomes, returnedHomes.Count);
    }

    private async Task SeedHomes(params Home[] homes)
    {
        using var scope = _factory.Services.CreateScope();
        var repo = (InMemoryHomeRepository)_factory.Services.GetRequiredService<IHomesRepository>();
        repo.Clear();
        
        foreach (var home in homes)
        {
            await repo.AddAsync(home);
        }
    }
}