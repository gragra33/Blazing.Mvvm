using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components.Parameter;

/// <summary>
/// Provides an implementation of <see cref="IParameterResolver"/> that resolves and sets parameters on Views and ViewModels in a Blazor MVVM application.
/// </summary>
internal sealed class ParameterResolver : IParameterResolver
{
    private static readonly ConcurrentDictionary<Type, Lazy<IDictionary<string, PropertySetter>>> _cachedTypeProperties = new();
    private readonly ParameterResolutionMode _parameterResolutionMode;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterResolver"/> class with the specified resolution mode.
    /// </summary>
    /// <param name="parameterResolutionMode">The mode for resolving parameters.</param>
    public ParameterResolver(ParameterResolutionMode parameterResolutionMode)
    {
        _parameterResolutionMode = parameterResolutionMode;
    }

    /// <summary>
    /// Sets parameters from the given <see cref="ParameterView"/> on both the specified View and its ViewModel, according to the configured resolution mode.
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
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> or <paramref name="viewModel"/> is <c>null</c>.</exception>
    public bool SetParameters<TView, TViewModel>(TView view, TViewModel viewModel, in ParameterView parameters)
        where TView : IView<TViewModel>
        where TViewModel : IViewModelBase
    {
        ArgumentNullException.ThrowIfNull(view);
        ArgumentNullException.ThrowIfNull(viewModel);

        switch (_parameterResolutionMode)
        {
            case ParameterResolutionMode.ViewModel:
                SetViewModelParameters(viewModel, parameters);
                return true;

            case ParameterResolutionMode.ViewAndViewModel:
                parameters.SetParameterProperties(view);
                SetViewModelParameters(viewModel, parameters);
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Sets parameters on the specified ViewModel from the given <see cref="ParameterView"/>.
    /// </summary>
    /// <typeparam name="T">The type of the ViewModel.</typeparam>
    /// <param name="target">The ViewModel instance.</param>
    /// <param name="parameters">The parameters to set.</param>
    private static void SetViewModelParameters<T>(T target, in ParameterView parameters)
        where T : IViewModelBase
    {
        var properties = GetProperties(target.GetType());

        if (properties.Count == 0)
        {
            return;
        }

        foreach (var parameter in parameters)
        {
            if (properties.TryGetValue(parameter.Name, out var propertySetter))
            {
                propertySetter.SetValue(target, parameter.Value);
            }
        }
    }

    /// <summary>
    /// Gets a dictionary of property setters for the specified type, caching the result for future use.
    /// </summary>
    /// <param name="type">The type to inspect for parameter properties.</param>
    /// <returns>A dictionary mapping parameter names to <see cref="PropertySetter"/> instances.</returns>
    /// <exception cref="InvalidOperationException">Thrown if duplicate parameter names are found on the type.</exception>
    private static IDictionary<string, PropertySetter> GetProperties(Type type)
    {
        return _cachedTypeProperties
            .GetOrAdd(type, key => new Lazy<IDictionary<string, PropertySetter>>(() => GetPropertySetters(key)))
            .Value;

        static IDictionary<string, PropertySetter> GetPropertySetters(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (properties.Length == 0)
            {
                return ImmutableDictionary<string, PropertySetter>.Empty;
            }

            var propertySetters = new Dictionary<string, PropertySetter>(StringComparer.OrdinalIgnoreCase);

            foreach (var propertyInfo in properties)
            {
                var attribute = propertyInfo.GetCustomAttribute<ViewParameterAttribute>();

                if (attribute is null)
                {
                    continue;
                }

                var propertySetter = new PropertySetter(type, propertyInfo);

                var key = attribute.Name ?? propertyInfo.Name;

                if (!propertySetters.TryAdd(key, propertySetter))
                {
                    throw new InvalidOperationException($"Duplicate parameter name '{key}' found on type '{type.FullName}'.");
                }
            }

            return propertySetters.Count > 0
                ? propertySetters
                : ImmutableDictionary<string, PropertySetter>.Empty;
        }
    }
}
