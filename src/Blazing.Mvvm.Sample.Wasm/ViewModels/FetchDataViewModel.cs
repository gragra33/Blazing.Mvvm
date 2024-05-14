using System.Collections.ObjectModel;
using System.Net.Http.Json;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.Wasm.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class FetchDataViewModel : ViewModelBase, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FetchDataViewModel> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    private ObservableCollection<WeatherForecast> _weatherForecasts = new();

    public FetchDataViewModel(HttpClient httpClient, ILogger<FetchDataViewModel> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public override async Task Loaded()
    {
        await Task.Delay(1000, _cancellationTokenSource.Token);
        var weatherForecasts = await _httpClient.GetFromJsonAsync<ObservableCollection<WeatherForecast>>("sample-data/weather.json", _cancellationTokenSource.Token);

        if (weatherForecasts is null)
        {
            return;
        }

        WeatherForecasts = weatherForecasts;
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing FetchDataViewModel");
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}