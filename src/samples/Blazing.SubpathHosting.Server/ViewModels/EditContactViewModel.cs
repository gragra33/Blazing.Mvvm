using System.ComponentModel;
using Blazing.Mvvm.ComponentModel;
using Blazing.SubpathHosting.Server.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.SubpathHosting.Server.ViewModels;

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
        => _logger.LogInformation("Form is valid and submitted!");

    private void ContactOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => NotifyStateChanged();
}
