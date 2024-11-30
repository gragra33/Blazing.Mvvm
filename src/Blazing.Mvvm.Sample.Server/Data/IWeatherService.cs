using Blazing.Mvvm.Sample.Server.Models;

namespace Blazing.Mvvm.Sample.Server.Data;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>?> GetForecastAsync(CancellationToken cancellationToken = default);
}
