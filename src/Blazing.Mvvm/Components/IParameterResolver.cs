using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Defines a mechanism for resolving and setting parameters on a target View and its ViewModel in a Blazor MVVM application.
/// </summary>
public interface IParameterResolver
{
    /// <summary>
    /// Sets the parameters from the given <see cref="ParameterView"/> on both the specified <paramref name="view"/> and its <paramref name="viewModel"/>.
    /// </summary>
    /// <typeparam name="TView">The type of the View. Must implement <see cref="IView{TViewModel}"/>.</typeparam>
    /// <typeparam name="TViewModel">The type of the ViewModel. Must implement <see cref="IViewModelBase"/>.</typeparam>
    /// <param name="view">The view on which to set the parameters.</param>
    /// <param name="viewModel">The ViewModel on which to set the parameters.</param>
    /// <param name="parameters">The parameters to set on the View and ViewModel.</param>
    /// <returns>
    /// <c>true</c> if the parameters were set successfully; otherwise, <c>false</c>.
    /// Returns <c>false</c> if the <see cref="ParameterResolutionMode"/> is set to <see cref="ParameterResolutionMode.None"/> or is not a valid value; otherwise, returns <c>true</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> or <paramref name="viewModel"/> is <see langword="null"/>.</exception>
    bool SetParameters<TView, TViewModel>(TView view, TViewModel viewModel, in ParameterView parameters)
        where TView : IView<TViewModel>
        where TViewModel : IViewModelBase;
}
