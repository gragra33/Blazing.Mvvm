using Blazing.Mvvm.Components;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.ComponentTests;

/// <summary>
/// Component tests verifying that Views dispose the transient ViewModels they own, and only those.
/// </summary>
public class TransientViewModelDisposalTests : BunitContext
{
    /// <summary>
    /// Tests that a View disposes its transient ViewModel when the View is disposed.
    /// </summary>
    [Fact]
    public async Task GivenTransientViewModel_WhenViewDisposed_ThenViewModelDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var viewModel = Render<DisposalTestView>().Instance.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that each View instance gets its own transient ViewModel.
    /// </summary>
    [Fact]
    public void GivenTransientViewModel_WhenTwoViewsRendered_ThenEachOwnsDistinctViewModel()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();

        // Act
        var first = Render<DisposalTestView>().Instance.CurrentViewModel;
        var second = Render<DisposalTestView>().Instance.CurrentViewModel;

        // Assert
        first.ShouldNotBeSameAs(second);
    }

    /// <summary>
    /// Tests that a View does not dispose a ViewModel whose lifetime the container owns.
    /// </summary>
    /// <param name="lifetime">The lifetime the ViewModel is registered with.</param>
    [Theory]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Singleton)]
    public async Task GivenNonTransientViewModel_WhenViewDisposed_ThenViewModelNotDisposed(ServiceLifetime lifetime)
    {
        // Arrange
        AddMvvm();
        Services.Add(ServiceDescriptor.Describe(typeof(IDisposalTrackingViewModel), typeof(DisposalTrackingViewModel), lifetime));
        var viewModel = Render<DisposalTestView>().Instance.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that disabling <see cref="LibraryConfiguration.DisposeTransientViewModels"/> leaves transient ViewModels to the container.
    /// </summary>
    [Fact]
    public async Task GivenOptionDisabled_WhenViewDisposed_ThenTransientViewModelNotDisposed()
    {
        // Arrange
        AddMvvm(isDisposeTransientViewModelsEnabled: false);
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var viewModel = Render<DisposalTestView>().Instance.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a View resolving a keyed transient ViewModel through <see cref="ViewModelKeyAttribute"/> disposes it.
    /// </summary>
    [Fact]
    public async Task GivenKeyedTransientViewModel_WhenViewDisposed_ThenViewModelDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddKeyedTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>(KeyedDisposalTestView.Key);
        var viewModel = Render<KeyedDisposalTestView>().Instance.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a layout disposes its transient ViewModel when the layout is disposed.
    /// </summary>
    [Fact]
    public async Task GivenTransientViewModel_WhenLayoutDisposed_ThenViewModelDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var viewModel = Render<DisposalTestLayout>().Instance.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a ViewModel assigned through the setter is not disposed by the View, even when it replaces one the View owned.
    /// </summary>
    [Fact]
    public async Task GivenViewModelAssignedThroughSetter_WhenViewDisposed_ThenAssignedViewModelNotDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var view = Render<DisposalTestView>().Instance;
        var assigned = new DisposalTrackingViewModel();
        view.AssignViewModel(assigned);

        // Act
        await DisposeComponentsAsync();

        // Assert
        assigned.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a View without <c>AddMvvm</c> registration resolves its transient ViewModel from the container as before.
    /// </summary>
    [Fact]
    public async Task GivenNoFactoryRegistered_WhenViewDisposed_ThenTransientViewModelNotDisposed()
    {
        // Arrange
        Services.AddSingleton<IParameterResolver>(new Blazing.Mvvm.Components.Parameter.ParameterResolver(ParameterResolutionMode.None));
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var viewModel = Render<DisposalTestView>().Instance.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Registers the Blazing.Mvvm services without scanning any assembly for ViewModels.
    /// </summary>
    /// <param name="isDisposeTransientViewModelsEnabled">The value for <see cref="LibraryConfiguration.DisposeTransientViewModels"/>.</param>
    private void AddMvvm(bool isDisposeTransientViewModelsEnabled = true)
        => Services.AddMvvm(configuration =>
        {
            configuration.RegisterViewModelsFromAssemblyContaining<BunitContext>();
            configuration.DisposeTransientViewModels = isDisposeTransientViewModelsEnabled;
        });

    /// <summary>
    /// A View that exposes its ViewModel to the test.
    /// </summary>
    public class DisposalTestView : MvvmComponentBase<IDisposalTrackingViewModel>
    {
        /// <summary>
        /// Gets the ViewModel the View resolved.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <summary>
        /// Replaces the View's ViewModel through the setter.
        /// </summary>
        /// <param name="viewModel">The ViewModel to assign.</param>
        public void AssignViewModel(IDisposalTrackingViewModel viewModel)
            => ViewModel = viewModel;
    }

    /// <summary>
    /// A View that resolves its ViewModel by key.
    /// </summary>
    [ViewModelKey(Key)]
    public class KeyedDisposalTestView : MvvmComponentBase<IDisposalTrackingViewModel>
    {
        /// <summary>
        /// The key the View resolves its ViewModel with.
        /// </summary>
        public const string Key = "DisposalTest";

        /// <summary>
        /// Gets the ViewModel the View resolved.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;
    }

    /// <summary>
    /// A layout that exposes its ViewModel to the test.
    /// </summary>
    public class DisposalTestLayout : MvvmLayoutComponentBase<IDisposalTrackingViewModel>
    {
        /// <summary>
        /// Gets the ViewModel the layout resolved.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;
    }
}
