using Blazing.Mvvm.Sample.Shared.Data;
using Blazing.Mvvm.Sample.Shared.Models;

namespace Blazing.Mvvm.Security.Wasm.Data;

/// <summary>
/// WebAssembly implementation of the weather service with mock data.
/// </summary>
public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    /// <inheritdoc/>
    public async Task<IEnumerable<WeatherForecast>?> GetForecastAsync(CancellationToken cancellationToken = default)
    {
        // Simulate async operation
        await Task.Delay(500, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).ToArray();
    }
}
