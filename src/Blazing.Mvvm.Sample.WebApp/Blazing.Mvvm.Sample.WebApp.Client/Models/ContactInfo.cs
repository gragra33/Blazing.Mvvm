using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Blazing.Mvvm.Sample.WebApp.Client.Models;

public class ContactInfo : ObservableValidator
{
    private string? _name;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value, true);
    }

    private string? _email;

    [Required]
    [EmailAddress]
    public string? Email
    {
        get => _email;
        set => SetProperty(ref _email, value, true);
    }

    private string? _phoneNumber;

    [Required]
    [Phone]
    public string? PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value, true);
    }

    public void Validate() => ValidateAllProperties();
}