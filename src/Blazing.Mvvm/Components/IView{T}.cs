using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Components;

// differentiate View (Page) from ViewModel for MvvmNavigationManager auto-detection
public interface IView<out TViewModel> : IView
    where TViewModel : IViewModelBase
{
    // Skip
}
