using System.Collections.ObjectModel;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.Server.Data;
using Blazing.Mvvm.Sample.Server.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class FetchDataViewModel : ViewModelBase, IDisposable
{
    private readonly WeatherForecastService _weatherForecastService;
    private readonly ILogger<FetchDataViewModel> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    private ObservableCollection<WeatherForecast> _weatherForecasts = [];

    public FetchDataViewModel(WeatherForecastService weatherForecastService, ILogger<FetchDataViewModel> logger)
    {
        _weatherForecastService = weatherForecastService;
        _logger = logger;
    }

    public override async Task Loaded()
    {
        var weatherForecasts = await _weatherForecastService.GetForecastAsync();

        if (weatherForecasts is null)
        {
            return;
        }

        WeatherForecasts = new ObservableCollection<WeatherForecast>(weatherForecasts);
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing FetchDataViewModel");
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}