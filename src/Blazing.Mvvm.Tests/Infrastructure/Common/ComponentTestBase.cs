using Blazing.Mvvm.ComponentModel;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Common;

/// <summary>
/// Base class for component tests providing common setup and helper methods for dependency injection and mocking.
/// </summary>
public abstract class ComponentTestBase : TestContext
{
    private readonly AutoMocker _autoMocker;

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentTestBase"/> class and configures the test context and service provider.
    /// </summary>
    protected ComponentTestBase()
    {
        _autoMocker = AutoMockerFactory();
        Services.AddFallbackServiceProvider(new MockServiceProvider(_autoMocker));
        
#if NET10_0
        // For .NET 10.0, manually register ComponentsMetrics service as singleton
        // This is required by the Blazor renderer in .NET 10.0
        try
        {
            var componentsMetricsType = Type.GetType("Microsoft.AspNetCore.Components.ComponentsMetrics, Microsoft.AspNetCore.Components");
            if (componentsMetricsType != null)
            {
                Services.AddSingleton(componentsMetricsType, _ => null!);
            }
        }
        catch
        {
            // Ignore if the type doesn't exist or can't be loaded
        }
#endif
    }

    /// <summary>
    /// Factory method for creating an <see cref="AutoMocker"/> instance. Can be overridden for custom behavior.
    /// </summary>
    /// <returns>A new <see cref="AutoMocker"/> instance.</returns>
    protected virtual AutoMocker AutoMockerFactory()
        => new(MockBehavior.Loose);

    /// <summary>
    /// Creates an instance of the specified type using <c>AutoMocker</c>. Optionally caches the instance for reuse.
    /// </summary>
    /// <typeparam name="T">The type of the class to create.</typeparam>
    /// <param name="cacheInstance">Specifies whether to cache the instance or not.</param>
    /// <returns>An instance of the specified type.</returns>
    protected T CreateInstance<T>(bool cacheInstance = false)
        where T : class
    {
        if (!cacheInstance)
        {
            return _autoMocker.CreateInstance<T>();
        }

        if (!_autoMocker.ResolvedObjects.ContainsKey(typeof(T)))
        {
            _autoMocker.With<T>();
        }

        return _autoMocker.Get<T>();
    }

    /// <summary>
    /// Gets a mock of the specified type from the <c>AutoMocker</c>.
    /// </summary>
    /// <typeparam name="T">The type of service mock to get.</typeparam>
    /// <returns>The requested mock of <typeparamref name="T"/>.</returns>
    protected Mock<T> GetMock<T>()
        where T : class
        => _autoMocker.GetMock<T>();

    /// <summary>
    /// Gets a service of the specified type from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <returns>A service object of type <typeparamref name="T"/>.</returns>
    protected T GetService<T>()
        where T : notnull
        => Services.GetRequiredService<T>();

    /// <summary>
    /// Gets a view model of the specified type from the service provider, optionally using a key.
    /// </summary>
    /// <typeparam name="T">The type of view model to get.</typeparam>
    /// <param name="key">The key for the view model.</param>
    /// <returns>A view model object of type <typeparamref name="T"/>.</returns>
    protected T GetViewModel<T>(object? key = null)
        where T : IViewModelBase
    {
        return key is null
            ? Services.GetRequiredService<T>()
            : Services.GetRequiredKeyedService<T>(key);
    }
}
