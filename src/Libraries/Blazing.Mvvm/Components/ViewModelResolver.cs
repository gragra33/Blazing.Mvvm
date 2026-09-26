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
        var vmKey = GetViewModelKey(view);

        if (vmKey is null)
        {
            return serviceProvider.GetRequiredService<TViewModel>();
        }

        return serviceProvider.GetRequiredKeyedService<TViewModel>(vmKey.Key);
    }

    /// <summary>
    /// Resolves a ViewModel instance for the specified View and reports whether the View owns it.
    /// A transient ViewModel is created by <see cref="TransientViewModelFactory"/> when it is registered, so the container does not
    /// hold it and the View must dispose it; any other ViewModel is resolved as <see cref="Resolve{TViewModel}(IView{TViewModel}, IServiceProvider)"/> does.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the ViewModel to resolve. Must implement <see cref="IViewModelBase"/>.</typeparam>
    /// <param name="view">The View for which to resolve the ViewModel.</param>
    /// <param name="serviceProvider">The service provider used for dependency resolution.</param>
    /// <param name="isOwnedByView">
    /// When this method returns, <see langword="true"/> if the View owns the ViewModel and must dispose it; otherwise, <see langword="false"/>.
    /// </param>
    /// <returns>The resolved ViewModel instance.</returns>
    public static TViewModel Resolve<TViewModel>(IView<TViewModel> view, IServiceProvider serviceProvider, out bool isOwnedByView)
        where TViewModel : IViewModelBase
    {
        if (serviceProvider.GetService<TransientViewModelFactory>() is { } factory
            && factory.TryCreate(typeof(TViewModel), GetViewModelKey(view)?.Key, serviceProvider, out var viewModel))
        {
            isOwnedByView = true;
            return (TViewModel)viewModel;
        }

        isOwnedByView = false;
        return Resolve(view, serviceProvider);
    }

    /// <summary>
    /// Gets the <see cref="ViewModelKeyAttribute"/> declared on the View's type, caching the lookup per type.
    /// </summary>
    /// <param name="view">The View whose type is inspected.</param>
    /// <returns>The View's <see cref="ViewModelKeyAttribute"/>, or <see langword="null"/> if it has none.</returns>
    private static ViewModelKeyAttribute? GetViewModelKey(IView view)
        => _vmKeyAttributes.GetOrAdd(view.GetType(), viewType => viewType.GetCustomAttribute<ViewModelKeyAttribute>());
}
