using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.Input;

namespace ParameterResolution.Sample.Wasm.ViewModels;

public sealed partial class HomeViewModel : ViewModelBase
{
    private readonly IMvvmNavigationManager _navigationManager;

    public HomeViewModel(IMvvmNavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    [RelayCommand]
    private void NavigateWithParams(string queryString)
    {
        _navigationManager.NavigateTo<ParameterDemoViewModel>(queryString);
    }
}
