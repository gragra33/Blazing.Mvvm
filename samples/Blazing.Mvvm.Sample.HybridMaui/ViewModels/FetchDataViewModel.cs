using Microsoft.Extensions.Logging;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.HybridMaui.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

[ViewModelDefinition]
public sealed partial class FetchDataViewModel(ILogger<FetchDataViewModel> logger) : ViewModelBase, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    private IEnumerable<WeatherForecast>? _weatherForecasts;

    public override async Task OnInitializedAsync()
    {
        await Task.Delay(1000, _cancellationTokenSource.Token);
        WeatherForecasts = new ObservableCollection<WeatherForecast>(Get());
    }

    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public IEnumerable<WeatherForecast> Get()
        => Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
            .ToArray();

    public void Dispose()
    {
        logger.LogInformation("Disposing {VMName}.", nameof(FetchDataViewModel));
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
