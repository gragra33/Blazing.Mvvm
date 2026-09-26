using Blazing.Mvvm.Components;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="TransientViewModelFactory"/> covering which registrations it replays and that the container does not track what it creates.
/// </summary>
public class TransientViewModelFactoryTests
{
    private readonly IServiceCollection _services = new ServiceCollection();

    /// <summary>
    /// Tests that the container itself keeps and disposes a transient ViewModel it resolves, which is the leak the factory avoids.
    /// </summary>
    [Fact]
    public void GivenTransientViewModelResolvedFromContainer_WhenProviderDisposed_ThenContainerDisposesViewModel()
    {
        // Arrange
        _services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var provider = _services.BuildServiceProvider();
        var viewModel = provider.GetRequiredService<IDisposalTrackingViewModel>();

        // Act
        provider.Dispose();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a transient ViewModel created by the factory is not tracked, so disposing the provider does not dispose it.
    /// </summary>
    [Fact]
    public void GivenTransientViewModel_WhenCreatedAndProviderDisposed_ThenContainerDoesNotTrackViewModel()
    {
        // Arrange
        _services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var sut = new TransientViewModelFactory(_services);
        var provider = _services.BuildServiceProvider();

        // Act
        var isCreated = sut.TryCreate(typeof(IDisposalTrackingViewModel), null, provider, out var viewModel);
        provider.Dispose();

        // Assert
        isCreated.ShouldBeTrue();
        viewModel.ShouldBeOfType<DisposalTrackingViewModel>().DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that the factory declines ViewModels registered with a lifetime other than transient.
    /// </summary>
    /// <param name="lifetime">The lifetime the ViewModel is registered with.</param>
    [Theory]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public void GivenNonTransientViewModel_WhenCreating_ThenDeclines(ServiceLifetime lifetime)
    {
        // Arrange
        _services.Add(ServiceDescriptor.Describe(typeof(IDisposalTrackingViewModel), typeof(DisposalTrackingViewModel), lifetime));
        var sut = new TransientViewModelFactory(_services);

        // Act
        var isCreated = sut.TryCreate(typeof(IDisposalTrackingViewModel), null, _services.BuildServiceProvider(), out var viewModel);

        // Assert
        isCreated.ShouldBeFalse();
        viewModel.ShouldBeNull();
    }

    /// <summary>
    /// Tests that the factory declines a ViewModel that is not registered.
    /// </summary>
    [Fact]
    public void GivenUnregisteredViewModel_WhenCreating_ThenDeclines()
    {
        // Arrange
        var sut = new TransientViewModelFactory(_services);

        // Act
        var isCreated = sut.TryCreate(typeof(IDisposalTrackingViewModel), null, _services.BuildServiceProvider(), out _);

        // Assert
        isCreated.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that the latest registration wins, matching the container, including registrations added after the factory was constructed.
    /// </summary>
    [Fact]
    public void GivenViewModelReregisteredAfterFactoryConstructed_WhenCreating_ThenUsesLatestRegistration()
    {
        // Arrange
        _services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var sut = new TransientViewModelFactory(_services);
        _services.AddSingleton<IDisposalTrackingViewModel, DisposalTrackingViewModel>();

        // Act
        var isCreated = sut.TryCreate(typeof(IDisposalTrackingViewModel), null, _services.BuildServiceProvider(), out _);

        // Assert
        isCreated.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that constructor dependencies are resolved from the supplied provider rather than created afresh.
    /// </summary>
    [Fact]
    public void GivenViewModelWithScopedDependency_WhenCreated_ThenSharesDependencyWithScope()
    {
        // Arrange
        _services.AddScoped<DisposalTrackingDependency>();
        _services.AddTransient<IDisposalTrackingViewModel, DependentDisposalTrackingViewModel>();
        var sut = new TransientViewModelFactory(_services);
        using var scope = _services.BuildServiceProvider().CreateScope();

        // Act
        sut.TryCreate(typeof(IDisposalTrackingViewModel), null, scope.ServiceProvider, out var viewModel);

        // Assert
        viewModel.ShouldBeOfType<DependentDisposalTrackingViewModel>().Dependency
            .ShouldBeSameAs(scope.ServiceProvider.GetRequiredService<DisposalTrackingDependency>());
    }

    /// <summary>
    /// Tests that a constructor parameter marked with <see cref="FromKeyedServicesAttribute"/> receives the keyed dependency.
    /// </summary>
    [Fact]
    public void GivenViewModelWithKeyedDependency_WhenCreated_ThenResolvesKeyedDependency()
    {
        // Arrange
        var dependency = new DisposalTrackingDependency();
        _services.AddKeyedSingleton("Dependency", dependency);
        _services.AddTransient<IDisposalTrackingViewModel, KeyedDependentDisposalTrackingViewModel>();
        var sut = new TransientViewModelFactory(_services);

        // Act
        sut.TryCreate(typeof(IDisposalTrackingViewModel), null, _services.BuildServiceProvider(), out var viewModel);

        // Assert
        viewModel.ShouldBeOfType<KeyedDependentDisposalTrackingViewModel>().Dependency.ShouldBeSameAs(dependency);
    }

    /// <summary>
    /// Tests that a factory registration is invoked to create the ViewModel.
    /// </summary>
    [Fact]
    public void GivenFactoryRegistration_WhenCreating_ThenInvokesFactory()
    {
        // Arrange
        var created = new DisposalTrackingViewModel();
        _services.AddTransient<IDisposalTrackingViewModel>(_ => created);
        var sut = new TransientViewModelFactory(_services);

        // Act
        sut.TryCreate(typeof(IDisposalTrackingViewModel), null, _services.BuildServiceProvider(), out var viewModel);

        // Assert
        viewModel.ShouldBeSameAs(created);
    }

    /// <summary>
    /// Tests that a keyed transient ViewModel is created only for its own key.
    /// </summary>
    [Fact]
    public void GivenKeyedTransientViewModel_WhenCreating_ThenMatchesOnKey()
    {
        // Arrange
        _services.AddKeyedTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>("Key");
        var sut = new TransientViewModelFactory(_services);
        var provider = _services.BuildServiceProvider();

        // Act
        var isCreatedForKey = sut.TryCreate(typeof(IDisposalTrackingViewModel), "Key", provider, out var viewModel);
        var isCreatedWithoutKey = sut.TryCreate(typeof(IDisposalTrackingViewModel), null, provider, out _);

        // Assert
        isCreatedForKey.ShouldBeTrue();
        viewModel.ShouldBeOfType<DisposalTrackingViewModel>();
        isCreatedWithoutKey.ShouldBeFalse();
    }

    /// <summary>
    /// Tests that a keyed factory registration receives the requested key.
    /// </summary>
    [Fact]
    public void GivenKeyedFactoryRegistration_WhenCreating_ThenPassesKeyToFactory()
    {
        // Arrange
        object? receivedKey = null;
        _services.AddKeyedTransient<IDisposalTrackingViewModel>("Key", (_, key) =>
        {
            receivedKey = key;
            return new DisposalTrackingViewModel();
        });
        var sut = new TransientViewModelFactory(_services);

        // Act
        sut.TryCreate(typeof(IDisposalTrackingViewModel), "Key", _services.BuildServiceProvider(), out _);

        // Assert
        receivedKey.ShouldBe("Key");
    }

    /// <summary>
    /// Tests that a ViewModel which receives its own service key is left to the container, which is the only thing that can supply it.
    /// </summary>
    [Fact]
    public void GivenViewModelReceivingServiceKey_WhenCreating_ThenDeclines()
    {
        // Arrange
        _services.AddKeyedTransient<IDisposalTrackingViewModel, ServiceKeyDisposalTrackingViewModel>("Key");
        var sut = new TransientViewModelFactory(_services);

        // Act
        var isCreated = sut.TryCreate(typeof(IDisposalTrackingViewModel), "Key", _services.BuildServiceProvider(), out _);

        // Assert
        isCreated.ShouldBeFalse();
    }
}
