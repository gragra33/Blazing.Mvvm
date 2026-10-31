using System.Reflection;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Tests.Infrastructure.Fakes;
using Bunit;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
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
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that disposal waits for the ViewModel's asynchronous cleanup rather than relying on an already completed task.
    /// </summary>
    /// <param name="isLayout">Whether the ViewModel belongs to a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenGatedAsyncDisposableViewModel_WhenDisposing_ThenAwaitsViewModel(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, GatedAsyncDisposalTrackingViewModel>();
        var (component, current) = RenderTracked(isLayout);
        var viewModel = (GatedAsyncDisposalTrackingViewModel)current;

        // Act
        var disposal = ((IAsyncDisposable)component).DisposeAsync().AsTask();
        var isCompletedBeforeGate = disposal.IsCompleted;
        viewModel.Gate.SetResult();
        await disposal;

        // Assert
        isCompletedBeforeGate.ShouldBeFalse();
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a throwing <c>Dispose(bool)</c> override propagates and still releases the ViewModel, asynchronously when it supports that.
    /// </summary>
    /// <param name="isLayout">Whether the ViewModel belongs to a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenThrowingDisposeOverride_WhenDisposingAsync_ThenAsyncViewModelStillDisposed(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, AsyncDisposalTrackingViewModel>();
        IComponent component = isLayout ? Render<ThrowingDisposeTestLayout>().Instance : Render<ThrowingDisposeTestView>().Instance;
        var viewModel = (AsyncDisposalTrackingViewModel)((ITrackedComponent)component).CurrentViewModel;
        ((ITrackedComponent)component).IsThrowing = true;

        // Act
        await Should.ThrowAsync<InvalidOperationException>(((IAsyncDisposable)component).DisposeAsync().AsTask());

        // Assert
        viewModel.DisposeAsyncCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a ViewModel whose cleanup re-enters the component's disposal is disposed once.
    /// </summary>
    /// <param name="isLayout">Whether the ViewModel belongs to a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenViewModelReenteringDisposal_WhenDisposingAsync_ThenDisposedOnce(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, ReentrantDisposalTrackingViewModel>();
        var (component, current) = RenderTracked(isLayout);
        var viewModel = (ReentrantDisposalTrackingViewModel)current;
        viewModel.OnDispose = () => ((IDisposable)component).Dispose();

        // Act
        await ((IAsyncDisposable)component).DisposeAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that repeated disposal, in either order of the two interfaces, disposes the ViewModel once.
    /// </summary>
    /// <param name="isLayout">Whether the ViewModel belongs to a layout rather than a View.</param>
    /// <param name="isSynchronousFirst">Whether <see cref="IDisposable.Dispose"/> is the first call.</param>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task GivenRepeatedDisposal_WhenDisposing_ThenViewModelDisposedOnce(bool isLayout, bool isSynchronousFirst)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var (component, viewModel) = RenderTracked(isLayout);

        // Act
        if (isSynchronousFirst)
        {
            ((IDisposable)component).Dispose();
        }
        else
        {
            await ((IAsyncDisposable)component).DisposeAsync();
        }

        await ((IAsyncDisposable)component).DisposeAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that an async-disposable ViewModel replaced through the setter is still disposed asynchronously, and the replacement not at all.
    /// </summary>
    [Fact]
    public async Task GivenAsyncDisposableViewModelReplacedThroughSetter_WhenViewDisposed_ThenOriginalDisposedAsynchronously()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, AsyncDisposalTrackingViewModel>();
        var view = Render<DisposalTestView>().Instance;
        var original = (AsyncDisposalTrackingViewModel)view.CurrentViewModel;
        var assigned = new DisposalTrackingViewModel();
        view.AssignViewModel(assigned);

        // Act
        await DisposeComponentsAsync();

        // Assert
        original.DisposeAsyncCount.ShouldBe(1);
        assigned.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that clearing the ViewModel through the setter does not orphan the one the component owns; the next one comes from the container.
    /// </summary>
    /// <param name="isLayout">Whether the ViewModel belongs to a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenViewModelClearedThroughSetter_WhenDisposed_ThenFirstDisposedAndSecondLeftToContainer(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        IDisposalTrackingViewModel first;
        IDisposalTrackingViewModel second;
        if (isLayout)
        {
            var layout = Render<DisposalTestLayout>().Instance;
            first = layout.CurrentViewModel;
            layout.AssignViewModel(null!);
            second = layout.CurrentViewModel;
        }
        else
        {
            var view = Render<DisposalTestView>().Instance;
            first = view.CurrentViewModel;
            view.AssignViewModel(null!);
            second = view.CurrentViewModel;
        }

        // Act
        await DisposeComponentsAsync();

        // Assert
        second.ShouldNotBeSameAs(first);
        first.DisposeCount.ShouldBe(1);
        second.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that disposing a component that was never initialized resolves no ViewModel.
    /// </summary>
    /// <param name="isLayout">Whether the component is a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GivenComponentNeverInitialized_WhenDisposed_ThenNoViewModelResolved(bool isLayout)
    {
        // Arrange
        AddMvvm();
        var counter = new DisposalTrackingCreationCounter();
        Services.AddSingleton(counter);
        Services.AddTransient<IDisposalTrackingViewModel, CountedDisposalTrackingViewModel>();
        IComponent component = isLayout ? new DisposalTestLayout() : new DisposalTestView();
        var baseType = isLayout ? typeof(MvvmLayoutComponentBase<IDisposalTrackingViewModel>) : typeof(MvvmComponentBase<IDisposalTrackingViewModel>);
        baseType.GetProperty("Services", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(component, Services);

        // Act
        ((IDisposable)component).Dispose();

        // Assert
        counter.Count.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a derived component's asynchronous cleanup runs while the ViewModel is usable, and that the component keeps owning its ViewModel.
    /// </summary>
    /// <param name="isLayout">Whether the component is a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenDisposeAsyncCoreOverride_WhenDisposed_ThenRunsBeforeViewModelDisposed(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        IComponent component = isLayout ? Render<AsyncCoreTestLayout>().Instance : Render<AsyncCoreTestView>().Instance;
        var tracked = (ITrackedComponent)component;
        var viewModel = tracked.CurrentViewModel;

        // Act
        await DisposeComponentsAsync();

        // Assert
        tracked.DisposeCountDuringCore.ShouldBe(0);
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that repeated disposal runs a derived component's asynchronous cleanup at most once, and not at all after a direct <c>Dispose()</c>.
    /// </summary>
    /// <param name="isLayout">Whether the component is a layout rather than a View.</param>
    /// <param name="isSynchronousFirst">Whether <see cref="IDisposable.Dispose"/> is the first call.</param>
    /// <param name="expectedCalls">The number of times the cleanup should run.</param>
    [Theory]
    [InlineData(false, false, 1)]
    [InlineData(false, true, 0)]
    [InlineData(true, false, 1)]
    [InlineData(true, true, 0)]
    public async Task GivenDisposeAsyncCoreOverride_WhenDisposedRepeatedly_ThenRunsAtMostOnce(bool isLayout, bool isSynchronousFirst, int expectedCalls)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        IComponent component = isLayout ? Render<AsyncCoreTestLayout>().Instance : Render<AsyncCoreTestView>().Instance;

        // Act
        if (isSynchronousFirst)
        {
            ((IDisposable)component).Dispose();
        }
        else
        {
            await ((IAsyncDisposable)component).DisposeAsync();
        }

        await ((IAsyncDisposable)component).DisposeAsync();

        // Assert
        ((ITrackedComponent)component).CoreCallCount.ShouldBe(expectedCalls);
    }

    /// <summary>
    /// Tests that a synchronous ViewModel is still released when a <c>Dispose(bool)</c> override throws before reaching the base class.
    /// </summary>
    /// <param name="isLayout">Whether the component is a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenThrowingDisposeOverride_WhenDisposingAsync_ThenSyncViewModelStillDisposed(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        IComponent component = isLayout ? Render<ThrowingDisposeTestLayout>().Instance : Render<ThrowingDisposeTestView>().Instance;
        var viewModel = ((ITrackedComponent)component).CurrentViewModel;
        ((ITrackedComponent)component).IsThrowing = true;

        // Act
        await Should.ThrowAsync<InvalidOperationException>(((IAsyncDisposable)component).DisposeAsync().AsTask());

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a re-entrant <c>DisposeAsync</c> from inside the asynchronous cleanup does nothing, and that a ViewModel
    /// resolved once disposal has started is left to the container.
    /// </summary>
    [Fact]
    public async Task GivenCleanupReenteringAndResolving_WhenDisposingAsync_ThenRunsOnceAndOwnsNothingNew()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var view = Render<ReenteringCoreTestView>().Instance;
        var original = view.CurrentViewModel;

        // Act
        await ((IAsyncDisposable)view).DisposeAsync();

        // Assert
        view.CoreCallCount.ShouldBe(1);
        original.DisposeCount.ShouldBe(1);
        view.ResolvedDuringCleanup.ShouldNotBeNull().ShouldNotBeSameAs(original);
        view.ResolvedDuringCleanup.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a ViewModel resolved once disposal has started is left to the container even when the owned ViewModel, being
    /// async-disposable, has already been detached and no longer blocks a second claim.
    /// </summary>
    [Fact]
    public async Task GivenAsyncDisposableViewModelDetached_WhenResolvingDuringCleanup_ThenOwnsNothingNew()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, AsyncDisposalTrackingViewModel>();
        var view = Render<ReenteringCoreTestView>().Instance;
        var original = (AsyncDisposalTrackingViewModel)view.CurrentViewModel;

        // Act
        await ((IAsyncDisposable)view).DisposeAsync();

        // Assert
        original.DisposeAsyncCount.ShouldBe(1);
        view.ResolvedDuringCleanup.ShouldNotBeNull().ShouldNotBeSameAs(original);
        view.ResolvedDuringCleanup.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that an owning View's synchronous cleanup and service scope are released when its asynchronous cleanup throws.
    /// </summary>
    [Fact]
    public async Task GivenOwningViewWithThrowingDisposeAsyncCore_WhenDisposed_ThenScopeStillReleased()
    {
        // Arrange
        AddMvvm();
        Services.AddScoped<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var view = Render<ThrowingCoreOwningTestView>().Instance;
        var viewModel = view.CurrentViewModel;

        // Act
        await Should.ThrowAsync<InvalidOperationException>(view.DisposeAsync().AsTask());

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a throwing <c>DisposeAsyncCore</c> override propagates and still runs the synchronous cleanup and releases the ViewModel.
    /// </summary>
    /// <param name="isLayout">Whether the component is a layout rather than a View.</param>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenThrowingDisposeAsyncCore_WhenDisposed_ThenViewModelStillDisposed(bool isLayout)
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        IComponent component = isLayout ? Render<AsyncCoreTestLayout>().Instance : Render<AsyncCoreTestView>().Instance;
        var tracked = (ITrackedComponent)component;
        var viewModel = tracked.CurrentViewModel;
        tracked.IsThrowing = true;

        // Act
        await Should.ThrowAsync<InvalidOperationException>(((IAsyncDisposable)component).DisposeAsync().AsTask());

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a layout with a factory forwarding to a shared instance never disposes that instance.
    /// </summary>
    [Fact]
    public async Task GivenTransientFactoryForwardingToSingleton_WhenLayoutDisposed_ThenSharedViewModelNotDisposed()
    {
        // Arrange
        AddMvvm();
        Services.AddSingleton<DisposalTrackingViewModel>();
        Services.AddTransient<IDisposalTrackingViewModel>(sp => sp.GetRequiredService<DisposalTrackingViewModel>());
        var shared = Services.GetRequiredService<DisposalTrackingViewModel>();
        Render<DisposalTestLayout>().Instance.CurrentViewModel.ShouldBeSameAs(shared);

        // Act
        await DisposeComponentsAsync();

        // Assert
        shared.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that a layout re-implementing <see cref="IAsyncDisposable"/> leaves its transient ViewModel to the container.
    /// </summary>
    [Fact]
    public async Task GivenLayoutReimplementingIAsyncDisposable_WhenScopeDisposed_ThenContainerDisposesViewModel()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>();
        var viewModel = Render<AsyncDisposeReimplementingTestLayout>().Instance.CurrentViewModel;
        await DisposeComponentsAsync();
        viewModel.DisposeCount.ShouldBe(0);

        // Act
        await ((IAsyncDisposable)Services).DisposeAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a ViewModel with two public constructors reaches its View through the container's choice of constructor.
    /// </summary>
    [Fact]
    public async Task GivenViewModelWithTwoPublicConstructors_WhenViewRendered_ThenContainerConstructsAndOwnsIt()
    {
        // Arrange
        AddMvvm();
        Services.AddSingleton<DisposalTrackingDependency>();
        Services.AddTransient<IDisposalTrackingViewModel, MarkedConstructorDisposalTrackingViewModel>();

        // Act
        var viewModel = (MarkedConstructorDisposalTrackingViewModel)Render<DisposalTestView>().Instance.CurrentViewModel;
        await DisposeComponentsAsync();

        // Assert
        viewModel.Dependency.ShouldNotBeNull();
        viewModel.DisposeCount.ShouldBe(0);
    }

    /// <summary>
    /// Tests that an open generic ViewModel falls back to the container, which disposes it when its scope ends.
    /// </summary>
    [Fact]
    public async Task GivenOpenGenericViewModel_WhenScopeDisposed_ThenContainerDisposesViewModel()
    {
        // Arrange
        AddMvvm();
        Services.AddTransient(typeof(OpenGenericDisposalTrackingViewModel<>));
        var viewModel = Render<OpenGenericTestView>().Instance.CurrentViewModel;
        await DisposeComponentsAsync();
        viewModel.DisposeCount.ShouldBe(0);

        // Act
        await ((IAsyncDisposable)Services).DisposeAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
    }

    /// <summary>
    /// Tests that a ViewModel registered under <see cref="KeyedService.AnyKey"/> falls back to the container, which disposes it when its scope ends.
    /// </summary>
    [Fact]
    public async Task GivenAnyKeyViewModel_WhenScopeDisposed_ThenContainerDisposesViewModel()
    {
        // Arrange
        AddMvvm();
        Services.AddKeyedTransient<IDisposalTrackingViewModel, DisposalTrackingViewModel>(KeyedService.AnyKey);
        var viewModel = Render<KeyedDisposalTestView>().Instance.CurrentViewModel;
        await DisposeComponentsAsync();
        viewModel.DisposeCount.ShouldBe(0);

        // Act
        await ((IAsyncDisposable)Services).DisposeAsync();

        // Assert
        viewModel.DisposeCount.ShouldBe(1);
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
    /// Renders a View or layout and returns it with its ViewModel.
    /// </summary>
    /// <param name="isLayout">Whether to render a layout rather than a View.</param>
    /// <returns>The rendered component and its ViewModel.</returns>
    private (IComponent Component, IDisposalTrackingViewModel ViewModel) RenderTracked(bool isLayout)
    {
        if (isLayout)
        {
            var layout = Render<DisposalTestLayout>().Instance;
            return (layout, layout.CurrentViewModel);
        }

        var view = Render<DisposalTestView>().Instance;
        return (view, view.CurrentViewModel);
    }

    /// <summary>
    /// A View or layout whose cleanup a test can observe or make throw.
    /// </summary>
    public interface ITrackedComponent
    {
        /// <summary>
        /// Gets the ViewModel the component resolved.
        /// </summary>
        IDisposalTrackingViewModel CurrentViewModel { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the component's cleanup throws.
        /// </summary>
        bool IsThrowing { get; set; }

        /// <summary>
        /// Gets the ViewModel's dispose count observed during <c>DisposeAsyncCore</c>, or -1 if it did not run.
        /// </summary>
        int DisposeCountDuringCore { get; }

        /// <summary>
        /// Gets the number of times <c>DisposeAsyncCore</c> ran.
        /// </summary>
        int CoreCallCount { get; }
    }

    /// <summary>
    /// A View whose <c>Dispose(bool)</c> override can throw.
    /// </summary>
    public class ThrowingDisposeTestView : MvvmComponentBase<IDisposalTrackingViewModel>, ITrackedComponent
    {
        /// <inheritdoc/>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <inheritdoc/>
        public bool IsThrowing { get; set; }

        /// <inheritdoc/>
        public int DisposeCountDuringCore => -1;

        /// <inheritdoc/>
        public int CoreCallCount => 0;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsThrowing)
            {
                IsThrowing = false;
                throw new InvalidOperationException("Cleanup failed");
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A layout whose <c>Dispose(bool)</c> override can throw.
    /// </summary>
    public class ThrowingDisposeTestLayout : MvvmLayoutComponentBase<IDisposalTrackingViewModel>, ITrackedComponent
    {
        /// <inheritdoc/>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <inheritdoc/>
        public bool IsThrowing { get; set; }

        /// <inheritdoc/>
        public int DisposeCountDuringCore => -1;

        /// <inheritdoc/>
        public int CoreCallCount => 0;

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (IsThrowing)
            {
                IsThrowing = false;
                throw new InvalidOperationException("Cleanup failed");
            }

            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// A View that adds asynchronous cleanup the supported way, by overriding <c>DisposeAsyncCore</c>.
    /// </summary>
    public class AsyncCoreTestView : MvvmComponentBase<IDisposalTrackingViewModel>, ITrackedComponent
    {
        /// <inheritdoc/>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <inheritdoc/>
        public bool IsThrowing { get; set; }

        /// <inheritdoc/>
        public int DisposeCountDuringCore { get; private set; } = -1;

        /// <inheritdoc/>
        public int CoreCallCount { get; private set; }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            CoreCallCount++;
            await Task.Yield();
            DisposeCountDuringCore = ViewModel.DisposeCount;
            if (IsThrowing)
            {
                throw new InvalidOperationException("Cleanup failed");
            }

            await base.DisposeAsyncCore();
        }
    }

    /// <summary>
    /// A layout that adds asynchronous cleanup the supported way, by overriding <c>DisposeAsyncCore</c>.
    /// </summary>
    public class AsyncCoreTestLayout : MvvmLayoutComponentBase<IDisposalTrackingViewModel>, ITrackedComponent
    {
        /// <inheritdoc/>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <inheritdoc/>
        public bool IsThrowing { get; set; }

        /// <inheritdoc/>
        public int DisposeCountDuringCore { get; private set; } = -1;

        /// <inheritdoc/>
        public int CoreCallCount { get; private set; }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            CoreCallCount++;
            await Task.Yield();
            DisposeCountDuringCore = ViewModel.DisposeCount;
            if (IsThrowing)
            {
                throw new InvalidOperationException("Cleanup failed");
            }

            await base.DisposeAsyncCore();
        }
    }

    /// <summary>
    /// A View whose asynchronous cleanup re-enters disposal and resolves another ViewModel.
    /// </summary>
    public class ReenteringCoreTestView : MvvmComponentBase<IDisposalTrackingViewModel>
    {
        /// <summary>
        /// Gets the ViewModel the View currently holds.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <summary>
        /// Gets the number of times the cleanup ran.
        /// </summary>
        public int CoreCallCount { get; private set; }

        /// <summary>
        /// Gets the ViewModel resolved while disposal was in progress.
        /// </summary>
        public IDisposalTrackingViewModel? ResolvedDuringCleanup { get; private set; }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsyncCore()
        {
            CoreCallCount++;
            await ((IAsyncDisposable)this).DisposeAsync();
            ViewModel = null!;
            ResolvedDuringCleanup = ViewModel;
            await base.DisposeAsyncCore();
        }
    }

    /// <summary>
    /// An owning View whose asynchronous cleanup throws before reaching the base class.
    /// </summary>
    public class ThrowingCoreOwningTestView : MvvmOwningComponentBase<IDisposalTrackingViewModel>
    {
        /// <summary>
        /// Gets the ViewModel the View resolved.
        /// </summary>
        public IDisposalTrackingViewModel CurrentViewModel => ViewModel;

        /// <inheritdoc/>
        protected override ValueTask DisposeAsyncCore()
            => throw new InvalidOperationException("Cleanup failed");
    }

    /// <summary>
    /// A layout that adds its own asynchronous disposal, as <c>@implements IAsyncDisposable</c> does.
    /// </summary>
    public class AsyncDisposeReimplementingTestLayout : MvvmLayoutComponentBase<IDisposalTrackingViewModel>, IAsyncDisposable
    {
        /// <summary>
        /// Gets the ViewModel the layout resolved.
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
    /// A View whose ViewModel closes an open generic registration.
    /// </summary>
    public class OpenGenericTestView : MvvmComponentBase<OpenGenericDisposalTrackingViewModel<int>>
    {
        /// <summary>
        /// Gets the ViewModel the View resolved.
        /// </summary>
        public OpenGenericDisposalTrackingViewModel<int> CurrentViewModel => ViewModel;
    }

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
