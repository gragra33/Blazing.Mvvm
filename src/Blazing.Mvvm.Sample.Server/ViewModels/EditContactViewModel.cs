using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.Sample.Server.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

public partial class EditContactViewModel : ViewModelBase
{
    [ObservableProperty]
    private ContactInfo _contact = new();

    [RelayCommand]
    private void Save()
    {
        Contact.Validate();
        Console.WriteLine(Contact.HasErrors
            ? "After validating, errors found!"
            : "Sending contact to server!");
    }


    [RelayCommand]
    protected void ClearForm()
        => Contact = new ContactInfo();
}