using System.Collections.ObjectModel;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.HybridMaui.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

public partial class FetchDataViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<WeatherForecast> _weatherForecasts = new();

    public override async Task Loaded()
        => WeatherForecasts = new ObservableCollection<WeatherForecast>(Get());

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
}