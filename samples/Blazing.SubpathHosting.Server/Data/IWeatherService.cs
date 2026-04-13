using Blazing.SubpathHosting.Server.Models;

namespace Blazing.SubpathHosting.Server.Data;

public interface IWeatherService
{
    Task<IEnumerable<WeatherForecast>?> GetForecastAsync(CancellationToken cancellationToken = default);
}
