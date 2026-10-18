using Blazing.Mvvm.Components;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Bunit;
using CommunityToolkit.Mvvm.Messaging;
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
    /// Tests that assigning a ViewModel through the setter leaves the assigned instance to its owner while the View still
    /// disposes the ViewModel it created.
    /// </summary>
    [Fact]
    public async Task GivenViewModelAssignedThroughSetter_WhenViewDisposed_ThenOriginalDisposedAndAssignedNot()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var view = Render<DisposalTestView>().Instance;
        var original = view.CurrentViewModel;
        var assigned = new DisposalTrackingViewModel();
        view.AssignViewModel(assigned);

        // Act
        await DisposeComponentsAsync();

        // Assert
        original.DisposeCount.ShouldBe(1);
        assigned.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a layout still disposes the ViewModel it created after another is assigned through the setter.
    /// </summary>
    [Fact]
    public async Task GivenViewModelAssignedThroughLayoutSetter_WhenLayoutDisposed_ThenOriginalDisposedAndAssignedNot()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var layout = Render<DisposalTestLayout>().Instance;
        var original = layout.CurrentViewModel;
        var assigned = new DisposalTrackingViewModel();
        layout.AssignViewModel(assigned);

        // Act
        await DisposeComponentsAsync();

        // Assert
        original.DisposeCount.ShouldBe(1);
        assigned.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that the View unsubscribes from and deactivates the ViewModel it initialized, not one assigned afterwards,
    /// so a strong-reference messenger does not keep the original alive.
    /// </summary>
    [Fact]
    public async Task GivenRecipientViewModelReplacedThroughSetter_WhenViewDisposed_ThenOriginalDeactivatedAndUnregistered()
    {
        // Arrange
        AddMvvm();
        var messenger = new StrongReferenceMessenger();
        Services.AddSingleton<IMessenger>(messenger);
        Services.AddTransient<IDisposalTrackingViewModel, RecipientDisposalTrackingViewModel>();
        var view = Render<DisposalTestView>().Instance;
        var original = (RecipientDisposalTrackingViewModel)view.CurrentViewModel;
        view.AssignViewModel(new DisposalTrackingViewModel());

        // Act
        await DisposeComponentsAsync();

        // Assert
        original.IsActive.ShouldBeFalse();
        messenger.IsRegistered<DisposalTrackingMessage>(original).ShouldBeFalse();
    }

    /// <summary>
    /// Tests that a transient factory forwarding to a shared instance is resolved by the container, so the View never disposes
    /// an instance other consumers still use.
    /// </summary>
    [Fact]
    public async Task GivenTransientFactoryForwardingToSingleton_WhenViewDisposed_ThenSharedViewModelNotDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddSingleton<DisposalTrackingViewModel>();
        Services.AddTransient<IDisposalTrackingViewModel>(sp => sp.GetRequiredService<DisposalTrackingViewModel>());
        var shared = Services.GetRequiredService<DisposalTrackingViewModel>();
        Render<DisposalTestView>().Instance.CurrentViewModel.ShouldBeSameAs(shared);

        // Act
        await DisposeComponentsAsync();

        // Assert
        shared.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a keyed transient factory forwarding to a scoped instance is resolved by the container.
    /// </summary>
    [Fact]
    public async Task GivenKeyedTransientFactoryForwardingToScoped_WhenViewDisposed_ThenSharedViewModelNotDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddScoped<DisposalTrackingViewModel>();
        Services.AddKeyedTransient<IDisposalTrackingViewModel>(KeyedDisposalTestView.Key, (sp, _) => sp.GetRequiredService<DisposalTrackingViewModel>());
        var shared = Services.GetRequiredService<DisposalTrackingViewModel>();
        Render<KeyedDisposalTestView>().Instance.CurrentViewModel.ShouldBeSameAs(shared);

        // Act
        await DisposeComponentsAsync();

        // Assert
        shared.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a transient ViewModel implementing <see cref="IAsyncDisposable"/> is disposed asynchronously when its View or layout closes.
    /// </summary>
    /// <param name="isLayout">Whether the ViewModel belongs to a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenAsyncDisposableTransientViewModel_WhenViewDisposed_ThenDisposedAsynchronously(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, AsyncDisposalTrackingViewModel>();
        var viewModel = (AsyncDisposalTrackingViewModel)(isLayout
            ? Render<DisposalTestLayout>().Instance.CurrentViewModel
            : Render<DisposalTestView>().Instance.CurrentViewModel);

        // Act
        await DisposeComponentsAsync();

        // Assert
        viewModel.DisposeAsyncCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a View re-implementing <see cref="IDisposable"/>, which bypasses the base class cleanup, still releases the
    /// transient ViewModel it owns.
    /// </summary>
    [Fact]
    public async Task GivenViewReimplementingIDisposable_WhenViewDisposed_ThenViewModelDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var view = Render<DisposeReimplementingTestView>().Instance;
        var viewModel = view.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        view.PageDisposeCount.ShouldBe(1);
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a View re-implementing <see cref="IAsyncDisposable"/>, whose disposal never reaches the base class, leaves its
    /// transient ViewModel to the container rather than owning one nothing would dispose.
    /// </summary>
    [Fact]
    public async Task GivenViewReimplementingIAsyncDisposable_WhenScopeDisposed_ThenContainerDisposesViewModel()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var viewModel = Render<AsyncDisposeReimplementingTestView>().Instance.CurrentViewModel;
        await DisposeComponentsAsync();

        // Act
        await ((IAsyncDisposable)Services).DisposeAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a View extending cleanup by overriding <see cref="MvvmComponentBase{TViewModel}"/>'s <c>Dispose(bool)</c> still has it called.
    /// </summary>
    [Fact]
    public async Task GivenViewOverridingDisposeBool_WhenViewDisposed_ThenOverrideCalledAndViewModelDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var view = Render<DisposeOverridingTestView>().Instance;
        var viewModel = view.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        view.OverrideCount.ShouldBe(1);
        viewModel.DisposeCount.ShouldBe(1);
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

        /// <summary>
        /// Replaces the layout's ViewModel through the setter.
        /// </summary>
        /// <param name="viewModel">The ViewModel to assign.</param>
        public void AssignViewModel(IDisposalTrackingViewModel viewModel)
            => ViewModel = viewModel;
    }

    /// <summary>
    /// A View that adds its own synchronous disposal, as <c>@implements IDisposable</c> with a <c>Dispose</c> method does.
    /// </summary>
    public class DisposeReimplementingTestView : MvvmComponentBase<IDisposalTrackingViewModel>, IDisposable
    {
        /// <summary>
        /// Gets the ViewModel the View resolved.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <summary>
        /// Gets the number of times the View's own <c>Dispose</c> was called.
        /// </summary>
        public int PageDisposeCount { get; private set; }

        /// <summary>
        /// Records the call without reaching the base class cleanup.
        /// </summary>
        public new void Dispose()
            => PageDisposeCount++;
    }

    /// <summary>
    /// A View that adds its own asynchronous disposal, as <c>@implements IAsyncDisposable</c> does.
    /// </summary>
    public class AsyncDisposeReimplementingTestView : MvvmComponentBase<IDisposalTrackingViewModel>, IAsyncDisposable
    {
        /// <summary>
        /// Gets the ViewModel the View resolved.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <summary>
        /// Completes without reaching the base class cleanup.
        /// </summary>
        /// <returns>A completed <see cref="ValueTask"/>.</returns>
        public ValueTask DisposeAsync()
            => ValueTask.CompletedTask;
    }

    /// <summary>
    /// A View that extends cleanup the supported way, by overriding <c>Dispose(bool)</c>.
    /// </summary>
    public class DisposeOverridingTestView : MvvmComponentBase<IDisposalTrackingViewModel>
    {
        /// <summary>
        /// Gets the ViewModel the View resolved.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <summary>
        /// Gets the number of times the override was called.
        /// </summary>
        public int OverrideCount { get; private set; }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            OverrideCount++;
            base.Dispose(disposing);
        }
    }
}
