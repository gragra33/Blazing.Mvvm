using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.HybridMaui.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

public sealed partial class EditContactViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<EditContactViewModel> _logger;

    [ObservableProperty]
    private ContactInfo _contact = new();

    public EditContactViewModel(ILogger<EditContactViewModel> logger)
    {
        _logger = logger;
        Contact.PropertyChanged += ContactOnPropertyChanged;
    }

    public void Dispose()
        => Contact.PropertyChanged -= ContactOnPropertyChanged;

    [RelayCommand]
    private void ClearForm()
        => Contact = new ContactInfo();

    [RelayCommand]
    private void Save()
    {
        Contact.Validate();
        var logMessage = Contact.HasErrors
            ? "Form has errors!"
            : "Form is valid and submitted!";
        _logger.LogInformation("{LogMessage}", logMessage);
    }

    private void ContactOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => NotifyStateChanged();
}
