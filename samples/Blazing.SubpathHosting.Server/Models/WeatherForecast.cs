using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.SubpathHosting.Server.Models;

public partial class WeatherForecast : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private int _temperatureC;

    partial void OnTemperatureCChanged(int value)
    {
        TemperatureF = 32 + (int)(value / 0.5556);
    }

    [ObservableProperty]
    private string? _summary;

    [ObservableProperty]
    private int _temperatureF;
}
