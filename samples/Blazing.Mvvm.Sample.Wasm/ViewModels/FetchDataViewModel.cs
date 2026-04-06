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
    private IEnumerable<WeatherForecast>? _weatherForecasts;

    public FetchDataViewModel(HttpClient httpClient, ILogger<FetchDataViewModel> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public override async Task OnInitializedAsync()
    {
        await Task.Delay(1000, _cancellationTokenSource.Token);
        WeatherForecasts = await _httpClient.GetFromJsonAsync<IEnumerable<WeatherForecast>>("sample-data/weather.json", _cancellationTokenSource.Token) ?? [];
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing {VMName}.", nameof(FetchDataViewModel));
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
