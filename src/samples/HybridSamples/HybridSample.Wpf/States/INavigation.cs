using Blazing.Mvvm.ComponentModel;

namespace HybridSample.Wpf.States;

/// <summary>
/// Defines navigation operations for the WPF application.
/// </summary>
public interface INavigation
{
    /// <summary>
    /// Navigates to the specified page by name or URL.
    /// </summary>
    /// <param name="page">The name or URL of the page to navigate to.</param>
    void NavigateTo(string page);

    /// <summary>
    /// Navigates to the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to.</typeparam>
    void NavigateTo<TViewModel>() where TViewModel : IViewModelBase;
}
