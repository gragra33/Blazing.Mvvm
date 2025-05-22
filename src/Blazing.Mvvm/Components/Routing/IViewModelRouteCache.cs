using System;
using System.Collections.Generic;

namespace Blazing.Mvvm.Components.Routing;

/// <summary>
/// Defines the contract for a cache that stores ViewModel to route mappings.
/// </summary>
public interface IViewModelRouteCache
{
    /// <summary>
    /// Gets the cached routes for ViewModels.
    /// </summary>
    IReadOnlyDictionary<Type, string> ViewModelRoutes { get; }

    /// <summary>
    /// Gets the cached routes for keyed ViewModels.
    /// </summary>
    IReadOnlyDictionary<object, string> KeyedViewModelRoutes { get; }
}
