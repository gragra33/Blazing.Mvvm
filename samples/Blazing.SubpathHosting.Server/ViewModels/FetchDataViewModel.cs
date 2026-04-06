using Blazing.Mvvm.ComponentModel;
using Blazing.SubpathHosting.Server.Data;
using Blazing.SubpathHosting.Server.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.SubpathHosting.Server.ViewModels;

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

    public override async Task OnInitializedAsync()
    {
        WeatherForecasts = await _weatherService.GetForecastAsync() ?? [];
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing {VMName}.", nameof(FetchDataViewModel));
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
