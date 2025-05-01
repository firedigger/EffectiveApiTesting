using EffectiveApiTesting.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace EffectiveApiTesting.Tests;

[TestClass]
public sealed class WeatherForecastControllerTests
{
    private static WebApplicationFactory<Program> _factory;
    private static HttpClient _client;
    private static SqliteConnection _connection;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(options =>
        {
            options.UseEnvironment("Testing");
            options.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ForecastDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                // Add a new DbContext with an in-memory database
                services.AddDbContext<ForecastDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ForecastDbContext>();
                db.Database.EnsureCreated();
            });
        });
        _client = _factory.CreateClient();
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static void Cleanup()
    {
        _connection?.Dispose();
        _factory?.Dispose();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ForecastDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    [TestMethod]
    public async Task ShouldGetWeatherForecast()
    {
        var response = await _client.GetAsync("/weatherforecast");
        response.EnsureSuccessStatusCode();
        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        Assert.IsNotNull(forecasts);
        Assert.AreEqual(5, forecasts.Length);
    }

    [TestMethod]
    public async Task ShouldGetWeatherForecastByDate()
    {
        var response = await _client.GetAsync("/weatherforecast/bydate?date=2024-05-01");
        response.EnsureSuccessStatusCode();
        var forecast = await response.Content.ReadFromJsonAsync<WeatherForecast>();
        Assert.IsNotNull(forecast);
        Assert.AreEqual(new DateOnly(2024, 5, 1), forecast.Date);
    }

    [TestMethod]
    public async Task ShouldFailWhenDateIsMissing()
    {
        var response = await _client.GetAsync("/weatherforecast/bydate");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(content, "date");
    }

    [TestMethod]
    public async Task ShouldFailWhenDateIsInvalid()
    {
        var response = await _client.GetAsync("/weatherforecast/bydate?date=20-30-FTM");
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(content, "date");
    }

    [TestMethod]
    public async Task ShouldPostForecastAsAdmin()
    {
        var expected = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Today),
            TemperatureC = 18,
            Summary = "Test entry"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, "/weatherforecast")
        {
            Content = JsonContent.Create(expected)
        };
        request.Headers.Add("Role", "Admin");
        var response = await _client.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var getResponse = await _client.GetAsync((string?)$"/weatherforecast/bydate?date={expected.Date:yyyy-MM-dd}");
        getResponse.EnsureSuccessStatusCode();
        var actual = await getResponse.Content.ReadFromJsonAsync<WeatherForecast>();
        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.Date, actual!.Date, "Date should match");
        Assert.AreEqual(expected.TemperatureC, actual.TemperatureC, "Temperature should match");
        Assert.AreEqual(expected.Summary, actual.Summary, "Summary should match");
    }

    [TestMethod]
    public async Task ShouldFailPostForecastAsUser()
    {
        Assert.AreEqual(HttpStatusCode.Forbidden, (await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/weatherforecast")
        {
            Content = JsonContent.Create(new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                TemperatureC = 22,
                Summary = "Should be denied"
            })
        })).StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ForecastDbContext>();
        var forecast = await db.Forecasts.FirstOrDefaultAsync(f => f.Summary == "Should be denied");
        Assert.IsNull(forecast, "Forecast should not be added");
    }
}
