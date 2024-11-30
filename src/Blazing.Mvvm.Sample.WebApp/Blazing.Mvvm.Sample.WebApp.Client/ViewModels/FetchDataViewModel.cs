using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.WebApp.Client.Data;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Sample.WebApp.Client.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class FetchDataViewModel : ViewModelBase, IDisposable
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<FetchDataViewModel> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    private IEnumerable<WeatherForecast>? _weatherForecasts;

    public FetchDataViewModel(IWeatherService weatherService, ILogger<FetchDataViewModel> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    public Task PersistStateAsync(PersistentComponentState state)
    {
        state.PersistAsJson(nameof(WeatherForecasts), WeatherForecasts);
        return Task.CompletedTask;
    }

    public async Task LoadStateAsync(PersistentComponentState state)
    {
        if (state.TryTakeFromJson<IEnumerable<WeatherForecast>>(nameof(WeatherForecasts), out var weatherForecasts))
        {
            WeatherForecasts = weatherForecasts!;
        }
        else
        {
            WeatherForecasts = await _weatherService.GetForecastAsync(_cancellationTokenSource.Token) ?? [];
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing {VMName}.", nameof(FetchDataViewModel));
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
