using Microsoft.EntityFrameworkCore;

namespace EffectiveApiTesting.Api;

public class ForecastDbContext(DbContextOptions<ForecastDbContext> options)
    : DbContext(options)
{
    public DbSet<WeatherForecast> Forecasts => Set<WeatherForecast>();
}
