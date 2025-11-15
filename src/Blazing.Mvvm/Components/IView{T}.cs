using Blazing.Mvvm.ComponentModel;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Represents a generic View in the MVVM pattern.
/// Used to differentiate the View (such as a Blazor Page or Component) from its ViewModel for <see cref="MvvmNavigationManager"/> auto-detection.
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel associated with this View. Must implement <see cref="IViewModelBase"/>.</typeparam>
public interface IView<out TViewModel> : IView
    where TViewModel : IViewModelBase
{
    // This interface is used for type safety and navigation detection. No members are defined.
}
