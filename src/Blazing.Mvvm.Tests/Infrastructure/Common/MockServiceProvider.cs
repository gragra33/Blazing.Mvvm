using Blazing.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Tests.Infrastructure.Common;

/// <summary>
/// A service provider implementation that uses <see cref="AutoMocker"/> to resolve services and view models for testing.
/// </summary>
public class MockServiceProvider : IServiceProvider
{
    private readonly AutoMocker _autoMocker;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockServiceProvider"/> class with the specified <see cref="AutoMocker"/>.
    /// </summary>
    /// <param name="autoMocker">The <see cref="AutoMocker"/> instance to use for resolving services.</param>
    public MockServiceProvider(AutoMocker autoMocker)
        => _autoMocker = autoMocker;

    /// <summary>
    /// Gets the service object of the specified type.
    /// Handles special cases for Blazor and view model types.
    /// </summary>
    /// <param name="serviceType">The type of service to get.</param>
    /// <returns>The requested service object, or <c>null</c> if not available.</returns>
    public object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IComponentActivator))
        {
            return null;
        }

#if NET10_0
        // Handle ComponentsMetrics service for .NET 10.0
        if (serviceType.Name == "ComponentsMetrics")
        {
            return null; // Return null to indicate it's not available
        }
#endif

        // Create real instances of ViewModels and not mock them.
        if (!serviceType.IsAbstract && serviceType.IsAssignableTo(typeof(IViewModelBase)) && !_autoMocker.ResolvedObjects.ContainsKey(serviceType))
        {
            _autoMocker.With(serviceType);
        }

        return _autoMocker.Get(serviceType);
    }
}
