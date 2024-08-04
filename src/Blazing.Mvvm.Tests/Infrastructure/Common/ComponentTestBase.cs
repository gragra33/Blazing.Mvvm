using Blazing.Mvvm.ComponentModel;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Common;

public abstract class ComponentTestBase : TestContext
{
    private readonly AutoMocker _autoMocker;

    protected ComponentTestBase()
    {
        _autoMocker = AutoMockerFactory();
        Services.AddFallbackServiceProvider(new MockServiceProvider(_autoMocker));
    }

    protected virtual AutoMocker AutoMockerFactory()
        => new(MockBehavior.Loose);

    /// <summary>
    /// Use this method to create an instance of a specified type with <c>AutoMocker</c>. This should ideally be used for creating instances of view models.
    /// <para>
    /// If <paramref name="cacheInstance"/> is <c>true</c>, the instance will be cached and reused.
    /// </para>
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
    /// Get a mock of the specified type from the <c>AutoMocker</c>.
    /// </summary>
    /// <typeparam name="T">The type of service mock to get.</typeparam>
    /// <returns>The requested mock of <typeparamref name="T"/>.</returns>
    protected Mock<T> GetMock<T>()
        where T : class
        => _autoMocker.GetMock<T>();

    /// <summary>
    /// Shortcut for <c>Services.GetRequiredService</c> to get a service from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <returns>A service object of type <typeparamref name="T"/>.</returns>
    protected T GetService<T>()
        where T : notnull
        => Services.GetRequiredService<T>();

    /// <summary>
    /// Shortcut for <c>Services.GetRequiredService</c> and <c>Services.GetRequiredKeyedService</c> to get a view model from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="key">The key for the view model.</param>
    /// <returns>A view model object of type <typeparamref name="T"/>.</returns>
    protected T GetViewModel<T>(string? key = null)
        where T : IViewModelBase
    {
        return !string.IsNullOrWhiteSpace(key)
            ? Services.GetRequiredKeyedService<T>(key)
            : Services.GetRequiredService<T>();
    }
}
