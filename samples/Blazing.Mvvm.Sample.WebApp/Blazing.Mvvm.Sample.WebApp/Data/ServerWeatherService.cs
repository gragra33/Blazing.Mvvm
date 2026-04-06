using Blazing.Mvvm.Sample.WebApp.Client.Data;
using Blazing.Mvvm.Sample.WebApp.Client.Models;

namespace Blazing.Mvvm.Sample.WebApp.Data;

public class ServerWeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public async Task<IEnumerable<WeatherForecast>?> GetForecastAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken);

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