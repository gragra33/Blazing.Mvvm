using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.AnalyzerTest.ViewModels;

// BLAZMVVM0005: Navigation to unregistered ViewModel
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public partial class Test005ViewModel : ViewModelBase
{
    private readonly IMvvmNavigationManager _navigation;

    public Test005ViewModel(IMvvmNavigationManager navigation)
    {
        _navigation = navigation;
    }

    [ObservableProperty]
    private string _message = "Test Navigation";

    [RelayCommand]
    private void NavigateToUnregistered()
    {
        // Navigating to unregistered ViewModel triggers BLAZMVVM0005
        _navigation.NavigateTo<UnregisteredTargetViewModel>();
    }
}
