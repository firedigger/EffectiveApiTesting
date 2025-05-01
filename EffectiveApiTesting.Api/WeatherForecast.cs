using System.Text.Json.Serialization;

namespace EffectiveApiTesting;

public class WeatherForecast
{
    [JsonIgnore]
    public int Id { get; set; }
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
