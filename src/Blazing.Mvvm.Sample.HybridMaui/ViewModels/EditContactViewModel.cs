using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Blazing.Mvvm.Sample.HybridMaui.Models;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

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