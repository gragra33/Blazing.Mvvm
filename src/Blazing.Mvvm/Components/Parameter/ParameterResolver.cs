using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Components.Parameter;

internal sealed class ParameterResolver : IParameterResolver
{
    private static readonly ConcurrentDictionary<Type, Lazy<IDictionary<string, PropertySetter>>> _cachedTypeProperties = new();
    private readonly ParameterResolutionMode _parameterResolutionMode;

    public ParameterResolver(ParameterResolutionMode parameterResolutionMode)
    {
        _parameterResolutionMode = parameterResolutionMode;
    }

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
