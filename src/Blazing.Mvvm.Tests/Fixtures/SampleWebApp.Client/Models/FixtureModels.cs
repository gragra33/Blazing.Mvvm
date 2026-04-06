using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.WebApp.Client.Models;

public class ContactInfo : ObservableValidator
{
    private string? _name;
    private string? _email;
    private string? _phoneNumber;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The {0} field must have a length between {2} and {1}.")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "The {0} field contains invalid characters. Only letters, spaces, apostrophes, and hyphens are allowed.")]
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    [Required]
    [EmailAddress]
    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value, true);
    }

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value, true);
    }
}

public partial class WeatherForecast : ObservableObject
{
    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private int _temperatureC;

    [ObservableProperty]
    private string? _summary;

    [ObservableProperty]
    private int _temperatureF;

    partial void OnTemperatureCChanged(int value)
        => TemperatureF = 32 + (int)(value / 0.5556);
}

public record ConvertHexToAsciiMessage(string HexToConvert);

public record ConvertAsciiToHexMessage(string AsciiToConvert);

public record ResetHexAsciiInputsMessage;
