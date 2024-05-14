using System.Collections.ObjectModel;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.WebApp.Client.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class WeatherViewModel : ViewModelBase, IDisposable
{
    private ILogger<WeatherViewModel> _logger;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [ObservableProperty]
    private ObservableCollection<WeatherForecast> _weatherForecasts = new();

    public WeatherViewModel(ILogger<WeatherViewModel> logger)
    {
        _logger = logger;
    }

    public override async Task Loaded()
    {
        await Task.Delay(1000, _cancellationTokenSource.Token);

        if (_cancellationTokenSource.Token.IsCancellationRequested)
        {
            return;
        }

        WeatherForecasts = new ObservableCollection<WeatherForecast>(Get());
    }

    public static IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });
    }

    public void Dispose()
    {
        _logger.LogInformation("Disposing WeatherViewModel");
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}