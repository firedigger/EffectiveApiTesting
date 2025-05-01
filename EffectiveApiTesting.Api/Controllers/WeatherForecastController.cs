using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EffectiveApiTesting.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ForecastDbContext dbContext) : ControllerBase
{
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get() => Enumerable.Range(1, 5).Select(index => new WeatherForecast
    {
        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        TemperatureC = Random.Shared.Next(-20, 55),
        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
    });

    [HttpGet("bydate", Name = "GetWeatherForecastByDate")]
    public async Task<WeatherForecast> GetByDate([FromQuery][Required] DateTime date) => await dbContext.Forecasts.FirstOrDefaultAsync(f => f.Date == DateOnly.FromDateTime(date)) ??
            new WeatherForecast
            {
                Date = DateOnly.FromDateTime(date),
                TemperatureC = 20,
                Summary = "Sample"
            };

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Add([FromBody] WeatherForecast forecast)
    {
        dbContext.Forecasts.Add(forecast);
        await dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = forecast.Id }, forecast);
    }

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];
}
