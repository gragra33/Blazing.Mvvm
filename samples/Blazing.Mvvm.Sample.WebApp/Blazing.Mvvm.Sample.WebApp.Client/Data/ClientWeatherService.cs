using System.Net.Http.Json;
using Blazing.Mvvm.Sample.WebApp.Client.Models;

namespace Blazing.Mvvm.Sample.WebApp.Client.Data;

public class ClientWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;

    public ClientWeatherService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<IEnumerable<WeatherForecast>?> GetForecastAsync(CancellationToken cancellationToken = default)
        => _httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>("/api/weatherforecast", cancellationToken);
}
