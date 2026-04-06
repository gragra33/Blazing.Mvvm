using Blazing.Mvvm.Sample.WebApp.Client.Models;

namespace Blazing.Mvvm.Sample.WebApp.Client.Data;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>?> GetForecastAsync(CancellationToken cancellationToken = default);
}
