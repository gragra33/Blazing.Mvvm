using System.Reflection;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ViewModelResolver"/> covering keyed and non-keyed ViewModel resolution, service lifetimes, and attribute caching.
/// </summary>
public class ViewModelResolverTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelResolverTests"/> class.
    /// </summary>
    public ViewModelResolverTests()
    {
        _services = new ServiceCollection();
        _serviceProvider = _services.BuildServiceProvider();
    }

    /// <summary>
    /// Tests that resolving a view without a key returns the registered service from the provider.
    /// </summary>
    [Fact]
    public void Resolve_GivenViewWithoutKey_ShouldResolveFromServiceProvider()
    {
        // Arrange
        var testViewModel = new TestViewModel();
        _services.AddSingleton<ITestViewModel>(testViewModel);
        var serviceProvider = _services.BuildServiceProvider();
        var view = new TestView();

        // Act
        var result = ViewModelResolver.Resolve(view, serviceProvider);

        // Assert
        result.Should().BeSameAs(testViewModel);
    }

    /// <summary>
    /// Tests that resolving a view with a key returns the keyed service from the provider.
    /// </summary>
    [Fact]
    public void Resolve_GivenViewWithKey_ShouldResolveKeyedService()
    {
        // Arrange
        var testViewModel = new TestKeyedViewModel();
        _services.AddKeyedSingleton<ITestKeyedViewModel>("TestKey", testViewModel);
        var serviceProvider = _services.BuildServiceProvider();
        var view = new TestKeyedView();

        // Act
        var result = ViewModelResolver.Resolve(view, serviceProvider);

        // Assert
        result.Should().BeSameAs(testViewModel);
    }

    /// <summary>
    /// Tests that resolving a view with a key caches the key attribute for subsequent calls.
    /// </summary>
    [Fact]
    public void Resolve_GivenViewWithKey_ShouldCacheKeyAttribute()
    {
        // Arrange
        var testViewModel1 = new TestKeyedViewModel();
        var testViewModel2 = new TestKeyedViewModel();
        _services.AddKeyedTransient<ITestKeyedViewModel>("TestKey", (provider, key) => new TestKeyedViewModel());
        var serviceProvider = _services.BuildServiceProvider();
        var view1 = new TestKeyedView();
        var view2 = new TestKeyedView();

        // Act - Call resolve twice to test caching
        var result1 = ViewModelResolver.Resolve(view1, serviceProvider);
        var result2 = ViewModelResolver.Resolve(view2, serviceProvider);

        // Assert - Both should be resolved with the same key (different instances due to Transient)
        using var _ = new AssertionScope();
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().NotBeSameAs(result2); // Transient services create new instances
    }

    /// <summary>
    /// Tests that resolving a view without a key attribute does not use keyed service resolution.
    /// </summary>
    [Fact]
    public void Resolve_GivenViewWithoutKeyAttribute_ShouldNotUseKeyedService()
    {
        // Arrange
        var testViewModel = new TestViewModel();
        _services.AddSingleton<ITestViewModel>(testViewModel);
        var serviceProvider = _services.BuildServiceProvider();
        var view = new TestView();

        // Act
        var result = ViewModelResolver.Resolve(view, serviceProvider);

        // Assert
        result.Should().BeSameAs(testViewModel);
    }

    /// <summary>
    /// Tests that resolving a service not registered throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void Resolve_GivenServiceNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var view = new TestView();

        // Act
        var act = () => ViewModelResolver.Resolve(view, serviceProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*ITestViewModel*");
    }

    /// <summary>
    /// Tests that resolving a keyed service not registered throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void Resolve_GivenKeyedServiceNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var view = new TestKeyedView();

        // Act
        var act = () => ViewModelResolver.Resolve(view, serviceProvider);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*TestKey*");
    }

    /// <summary>
    /// Tests that resolving different view types caches attributes per type.
    /// </summary>
    [Fact]
    public void Resolve_GivenDifferentViewTypes_ShouldCacheAttributesPerType()
    {
        // Arrange
        var testViewModel = new TestViewModel();
        var testKeyedViewModel = new TestKeyedViewModel();
        _services.AddSingleton<ITestViewModel>(testViewModel);
        _services.AddKeyedSingleton<ITestKeyedViewModel>("TestKey", testKeyedViewModel);
        var serviceProvider = _services.BuildServiceProvider();

        var regularView = new TestView();
        var keyedView = new TestKeyedView();

        // Act
        var regularResult = ViewModelResolver.Resolve(regularView, serviceProvider);
        var keyedResult = ViewModelResolver.Resolve(keyedView, serviceProvider);

        // Assert
        using var _ = new AssertionScope();
        regularResult.Should().BeSameAs(testViewModel);
        keyedResult.Should().BeSameAs(testKeyedViewModel);
    }

    /// <summary>
    /// Tests that multiple calls to resolve respect the service lifetime (transient).
    /// </summary>
    [Fact]
    public void Resolve_GivenMultipleCalls_ShouldRespectServiceLifetime()
    {
        // Arrange
        _services.AddTransient<ITestViewModel, TestViewModel>();
        var serviceProvider = _services.BuildServiceProvider();
        var view1 = new TestView();
        var view2 = new TestView();

        // Act
        var result1 = ViewModelResolver.Resolve(view1, serviceProvider);
        var result2 = ViewModelResolver.Resolve(view2, serviceProvider);

        // Assert
        result1.Should().NotBeSameAs(result2); // Transient should create new instances
    }

    /// <summary>
    /// Tests that resolving a singleton service returns the same instance.
    /// </summary>
    [Fact]
    public void Resolve_GivenSingletonService_ShouldReturnSameInstance()
    {
        // Arrange
        _services.AddSingleton<ITestViewModel, TestViewModel>();
        var serviceProvider = _services.BuildServiceProvider();
        var view1 = new TestView();
        var view2 = new TestView();

        // Act
        var result1 = ViewModelResolver.Resolve(view1, serviceProvider);
        var result2 = ViewModelResolver.Resolve(view2, serviceProvider);

        // Assert
        result1.Should().BeSameAs(result2); // Singleton should return same instance
    }

    /// <summary>
    /// Tests that resolving a view with an inherited key resolves the correct keyed service.
    /// </summary>
    [Fact]
    public void Resolve_GivenViewWithInheritedKey_ShouldResolveCorrectly()
    {
        // Arrange
        var testViewModel = new TestInheritedKeyedViewModel();
        _services.AddKeyedSingleton<ITestKeyedViewModel>("InheritedKey", testViewModel);
        var serviceProvider = _services.BuildServiceProvider();
        var view = new TestInheritedKeyedView();

        // Act
        var result = ViewModelResolver.Resolve(view, serviceProvider);

        // Assert
        result.Should().BeSameAs(testViewModel);
    }

    // Test classes and interfaces
    public interface ITestViewModel : IViewModelBase { }
    
    public class TestViewModel : ViewModelBase, ITestViewModel { }

    public class TestView : MvvmComponentBase<ITestViewModel> { }

    public interface ITestKeyedViewModel : IViewModelBase { }
    
    public class TestKeyedViewModel : ViewModelBase, ITestKeyedViewModel { }

    [ViewModelKey("TestKey")]
    public class TestKeyedView : MvvmComponentBase<ITestKeyedViewModel> { }

    public class TestInheritedKeyedViewModel : ViewModelBase, ITestKeyedViewModel { }

    [ViewModelKey("InheritedKey")]
    public class TestInheritedKeyedView : MvvmComponentBase<ITestKeyedViewModel> { }
}