using System.Collections.Concurrent;
using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Components;

/// <summary>
/// Provides static methods to resolve ViewModel instances for Blazor Views, supporting both keyed and non-keyed ViewModels.
/// </summary>
internal static class ViewModelResolver
{
    /// <summary>
    /// Caches <see cref="ViewModelKeyAttribute"/> instances for View types to optimize keyed ViewModel resolution.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, ViewModelKeyAttribute?> _vmKeyAttributes = [];

    /// <summary>
    /// Resolves a ViewModel instance for the specified View, using the provided <see cref="IServiceProvider"/>.
    /// If the View has a <see cref="ViewModelKeyAttribute"/>, resolves a keyed ViewModel; otherwise, resolves a standard ViewModel.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel to resolve. Must implement <see cref="IViewModelBase"/>.</typeparam>
    /// <param name="view">The View for which to resolve the ViewModel.</param>
    /// <param name="serviceProvider">The service provider used for dependency resolution.</param>
    /// <returns>The resolved ViewModel instance.</returns>
    /// <remarks>
    /// If the View is decorated with <see cref="ViewModelKeyAttribute"/>, the ViewModel is resolved using the associated key.
    /// Otherwise, the ViewModel is resolved as a standard service.
    /// </remarks>
    public static TViewModel Resolve<TViewModel>(IView<TViewModel> view, IServiceProvider serviceProvider)
        where TViewModel : IViewModelBase
    {
        var viewType = view.GetType();
        var vmKey = _vmKeyAttributes.GetOrAdd(viewType, key => key.GetCustomAttribute<ViewModelKeyAttribute>());

        if (vmKey is null)
        {
            return serviceProvider.GetRequiredService<TViewModel>();
        }

        return serviceProvider.GetRequiredKeyedService<TViewModel>(vmKey.Key);
    }
}
