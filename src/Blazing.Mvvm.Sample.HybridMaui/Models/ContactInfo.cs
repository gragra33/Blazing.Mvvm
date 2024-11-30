using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.HybridMaui.Models;

public class ContactInfo : ObservableValidator
{
    private string? _name;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "The {0} field must have a length between {2} and {1}.")]
    [RegularExpression(@"^[a-zA-Z\s'-]+$", ErrorMessage = "The {0} field contains invalid characters. Only letters, spaces, apostrophes, and hyphens are allowed.")]
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
    [Display(Name = "Phone Number")]
    public string? PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value, true);
    }

    public void Validate()
        => ValidateAllProperties();
}
