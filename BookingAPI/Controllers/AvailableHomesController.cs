using BookingAPI.Controllers.Models;
using BookingAPI.Data;
using BookingAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingAPI.Controllers;

[Route("api/available-homes")]
public class AvailableHomesController : ControllerBase
{
    private readonly IAvailableHomesService _availableHomesService;

    public AvailableHomesController(IAvailableHomesService availableHomesService)
    {
        ArgumentNullException.ThrowIfNull(availableHomesService);
        
        _availableHomesService = availableHomesService;
    }

    [HttpPost]
    public async Task<ActionResult<HomeOutputModel>> AddAsync([FromBody] HomeInputModel homeInputModel)
    {
        var home = await _availableHomesService.AddAsync(new Home()
        {
            Name = homeInputModel.Name,
            AvailableSlots = homeInputModel.AvailableSlots
        });
        
        return Ok(new HomeOutputModel()
        {
            Id = home.Id,
            Name = home.Name,
            AvailableSlots = home.AvailableSlots
        });
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HomeOutputModel>>> GetAvailableHomesAsync(DateOnly startDate, DateOnly endDate)
    {
        if (startDate == default || endDate == default)
        {
            return BadRequest("Start date and end date must be provided and valid");
        }
        
        if (startDate > endDate)
        {
            return BadRequest("Start date cannot be earlier than end date");
        }
        
        var availableHomes = await _availableHomesService.GetByDateRangeAsync(startDate, endDate);

        return Ok(availableHomes.Select(s => new HomeOutputModel()
        {
            Id = s.Id,
            Name = s.Name,
            AvailableSlots = s.AvailableSlots
        }));
    }
}