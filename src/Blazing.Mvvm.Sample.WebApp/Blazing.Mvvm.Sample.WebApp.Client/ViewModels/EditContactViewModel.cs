using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.WebApp.Client.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Sample.WebApp.Client.ViewModels;

public partial class EditContactViewModel : ViewModelBase
{
    private readonly ILogger<EditContactViewModel> _logger;

    [ObservableProperty]
    private ContactInfo _contact = new();

    public EditContactViewModel(ILogger<EditContactViewModel> logger)
    {
        _logger = logger;
    }

    [RelayCommand]
    private void Save()
    {
        Contact.Validate();
        var logMessage = Contact.HasErrors
            ? "After validating, errors found!"
            : "Sending contact to server!";
        _logger.LogInformation("{LogMessage}", logMessage);
    }

    [RelayCommand]
    protected void ClearForm()
        => Contact = new ContactInfo();
}