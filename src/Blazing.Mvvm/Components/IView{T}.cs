using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Components;

/// <summary>
/// An interface that represents a generic View.
/// Used to differentiate View (Page) from ViewModel for MvvmNavigationManager auto-detection
/// </summary>
/// <typeparam name="TViewModel">The ViewModel.</typeparam>
public interface IView<out TViewModel> : IView
    where TViewModel : IViewModelBase
{
    // Skip
}
