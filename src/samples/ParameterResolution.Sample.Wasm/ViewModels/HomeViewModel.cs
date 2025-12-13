using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.Input;

namespace ParameterResolution.Sample.Wasm.ViewModels;

/// <summary>
/// ViewModel for the Home page that demonstrates MVVM navigation with parameter passing.
/// </summary>
/// <remarks>
/// This ViewModel showcases how to use <see cref="IMvvmNavigationManager"/> with <see cref="RelayCommand"/>
/// to perform type-safe navigation while passing query string parameters from the View to the ViewModel.
/// </remarks>
public sealed partial class HomeViewModel : ViewModelBase
{
    private readonly IMvvmNavigationManager _navigationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="navigationManager">The MVVM navigation manager for type-safe navigation.</param>
    public HomeViewModel(IMvvmNavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Navigates to the <see cref="ParameterDemoViewModel"/> with the specified query string parameters.
    /// </summary>
    /// <param name="queryString">
    /// The query string containing parameters to pass to the destination ViewModel.
    /// Should start with '?' and include URL-encoded parameter values.
    /// </param>
    /// <example>
    /// Example query string: "?Title=Hello%20World&amp;Count=42&amp;Content=Sample%20Content"
    /// </example>
    [RelayCommand]
    private void NavigateWithParams(string queryString)
    {
        _navigationManager.NavigateTo<ParameterDemoViewModel>(queryString);
    }
}
