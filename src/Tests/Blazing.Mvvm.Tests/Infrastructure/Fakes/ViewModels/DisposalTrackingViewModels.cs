using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// A ViewModel that records how many times it has been disposed.
/// </summary>
public interface IDisposalTrackingViewModel : IViewModelBase
{
    /// <summary>
    /// Gets the number of times <see cref="IDisposable.Dispose"/> has been called.
    /// </summary>
    int DisposeCount { get; }
}

/// <summary>
/// A ViewModel that records how many times it has been disposed.
/// </summary>
public class DisposalTrackingViewModel : ViewModelBase, IDisposalTrackingViewModel
{
    /// <inheritdoc/>
    public int DisposeCount { get; private set; }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeCount++;
        }

        base.Dispose(disposing);
    }
}

/// <summary>
/// A disposal-tracking ViewModel with cleanup that only runs asynchronously.
/// </summary>
public sealed class AsyncDisposalTrackingViewModel : DisposalTrackingViewModel, IAsyncDisposable
{
    /// <summary>
    /// Gets the number of times <see cref="DisposeAsync"/> has been called.
    /// </summary>
    public int DisposeAsyncCount { get; private set; }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        DisposeAsyncCount++;
        Dispose();
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// A disposal-tracking ViewModel whose asynchronous disposal completes only when the test opens its gate.
/// </summary>
public sealed class GatedAsyncDisposalTrackingViewModel : DisposalTrackingViewModel, IAsyncDisposable
{
    /// <summary>
    /// Gets the gate that <see cref="DisposeAsync"/> waits on.
    /// </summary>
    public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await Gate.Task;
        Dispose();
    }
}

/// <summary>
/// A disposal-tracking ViewModel whose cleanup calls back once into the supplied action.
/// </summary>
public sealed class ReentrantDisposalTrackingViewModel : DisposalTrackingViewModel
{
    /// <summary>
    /// Gets or sets the action invoked by the first disposal.
    /// </summary>
    public Action? OnDispose { get; set; }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        var onDispose = OnDispose;
        OnDispose = null;
        onDispose?.Invoke();
    }
}

/// <summary>
/// Counts the ViewModels constructed with it.
/// </summary>
public sealed class DisposalTrackingCreationCounter
{
    /// <summary>
    /// Gets or sets the number of ViewModels constructed.
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// A disposal-tracking ViewModel that records its construction.
/// </summary>
public sealed class CountedDisposalTrackingViewModel : DisposalTrackingViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountedDisposalTrackingViewModel"/> class.
    /// </summary>
    /// <param name="counter">The counter to increment.</param>
    public CountedDisposalTrackingViewModel(DisposalTrackingCreationCounter counter)
        => counter.Count++;
}

/// <summary>
/// An open generic disposal-tracking ViewModel, which the container closes on request.
/// </summary>
/// <typeparam name="T">The closing type.</typeparam>
public sealed class OpenGenericDisposalTrackingViewModel<T> : DisposalTrackingViewModel;

/// <summary>
/// A disposal-tracking ViewModel with two public constructors, one of them marked for <see cref="ActivatorUtilities"/>, which the container ignores.
/// </summary>
public sealed class MarkedConstructorDisposalTrackingViewModel : DisposalTrackingViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkedConstructorDisposalTrackingViewModel"/> class.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public MarkedConstructorDisposalTrackingViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkedConstructorDisposalTrackingViewModel"/> class.
    /// </summary>
    /// <param name="dependency">The dependency the container supplies.</param>
    public MarkedConstructorDisposalTrackingViewModel(DisposalTrackingDependency dependency)
        => Dependency = dependency;

    /// <summary>
    /// Gets the dependency, which is set only when the container's choice of constructor was used.
    /// </summary>
    public DisposalTrackingDependency? Dependency { get; }
}

/// <summary>
/// A message used to register a recipient ViewModel with a messenger.
/// </summary>
public sealed record DisposalTrackingMessage;

/// <summary>
/// A disposal-tracking recipient ViewModel, activated by its View and registered with the supplied messenger while active.
/// </summary>
/// <param name="messenger">The messenger the ViewModel registers with when activated.</param>
public sealed class RecipientDisposalTrackingViewModel(IMessenger messenger)
    : RecipientViewModelBase(messenger), IDisposalTrackingViewModel, IRecipient<DisposalTrackingMessage>
{
    /// <inheritdoc/>
    public int DisposeCount { get; private set; }

    /// <inheritdoc/>
    public void Receive(DisposalTrackingMessage message)
    {
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeCount++;
        }

        base.Dispose(disposing);
    }
}

/// <summary>
/// A dependency registered with a scoped lifetime, used to prove a ViewModel shares its View's scope.
/// </summary>
public sealed class DisposalTrackingDependency;

/// <summary>
/// A disposal-tracking ViewModel with a constructor dependency.
/// </summary>
/// <param name="dependency">The dependency resolved from the View's service provider.</param>
public sealed class DependentDisposalTrackingViewModel(DisposalTrackingDependency dependency) : DisposalTrackingViewModel
{
    /// <summary>
    /// Gets the dependency the ViewModel was constructed with.
    /// </summary>
    public DisposalTrackingDependency Dependency { get; } = dependency;
}

/// <summary>
/// A disposal-tracking ViewModel with a keyed constructor dependency.
/// </summary>
/// <param name="dependency">The dependency registered under the key <c>"Dependency"</c>.</param>
public sealed class KeyedDependentDisposalTrackingViewModel([FromKeyedServices("Dependency")] DisposalTrackingDependency dependency) : DisposalTrackingViewModel
{
    /// <summary>
    /// Gets the dependency the ViewModel was constructed with.
    /// </summary>
    public DisposalTrackingDependency Dependency { get; } = dependency;
}

#if NET10_0_OR_GREATER
/// <summary>
/// A disposal-tracking ViewModel whose dependency inherits the key the ViewModel was resolved with.
/// </summary>
/// <param name="dependency">The dependency registered under the ViewModel's own key.</param>
public sealed class InheritedKeyDisposalTrackingViewModel([FromKeyedServices] DisposalTrackingDependency dependency) : DisposalTrackingViewModel
{
    /// <summary>
    /// Gets the dependency the ViewModel was constructed with.
    /// </summary>
    public DisposalTrackingDependency Dependency { get; } = dependency;
}
#endif

/// <summary>
/// A disposal-tracking ViewModel that receives its own service key, which only the container can supply.
/// </summary>
/// <param name="serviceKey">The key the ViewModel was resolved with.</param>
public sealed class ServiceKeyDisposalTrackingViewModel([ServiceKey] string serviceKey) : DisposalTrackingViewModel
{
    /// <summary>
    /// Gets the key the ViewModel was resolved with.
    /// </summary>
    public string ServiceKey { get; } = serviceKey;
}

/// <summary>
/// A disposal-tracking ViewModel whose <see cref="OnInitializedAsync"/> waits on a gate, and which records any lifecycle call
/// made after it was disposed. Such a call throws <see cref="ObjectDisposedException"/>, as it would for a ViewModel that
/// releases a resource in <c>Dispose(bool)</c> and uses it in its lifecycle methods.
/// </summary>
public class LifecycleRecordingViewModel : DisposalTrackingViewModel
{
    /// <summary>
    /// Gets the gate that <see cref="OnInitializedAsync"/> waits on.
    /// </summary>
    public TaskCompletionSource InitializationGate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Gets the lifecycle methods called after the ViewModel was disposed.
    /// </summary>
    public List<string> CallsAfterDisposal { get; } = [];

    /// <inheritdoc/>
    public override Task OnInitializedAsync()
        => InitializationGate.Task;

    /// <inheritdoc/>
    public override void OnParametersSet()
        => Record(nameof(OnParametersSet));

    /// <inheritdoc/>
    public override Task OnParametersSetAsync()
    {
        Record(nameof(OnParametersSetAsync));
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override bool ShouldRender()
    {
        Record(nameof(ShouldRender));
        return true;
    }

    /// <inheritdoc/>
    public override void OnAfterRender(bool firstRender)
        => Record(nameof(OnAfterRender));

    /// <inheritdoc/>
    public override Task OnAfterRenderAsync(bool firstRender)
    {
        Record(nameof(OnAfterRenderAsync));
        return Task.CompletedTask;
    }

    private void Record(string lifecycleMethod)
    {
        if (DisposeCount > 0)
        {
            CallsAfterDisposal.Add(lifecycleMethod);
            throw new ObjectDisposedException(nameof(LifecycleRecordingViewModel));
        }
    }
}

/// <summary>
/// A disposal-tracking ViewModel whose disposal throws after it has been recorded.
/// </summary>
public sealed class ThrowingDisposalTrackingViewModel : DisposalTrackingViewModel
{
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        throw new ApplicationException("ViewModel cleanup failed");
    }
}

/// <summary>
/// A disposal-tracking ViewModel whose asynchronous disposal throws after it has been recorded.
/// </summary>
public sealed class ThrowingAsyncDisposalTrackingViewModel : DisposalTrackingViewModel, IAsyncDisposable
{
    /// <summary>
    /// Gets the number of times <see cref="DisposeAsync"/> has been called.
    /// </summary>
    public int DisposeAsyncCount { get; private set; }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        DisposeAsyncCount++;
        throw new ApplicationException("ViewModel asynchronous cleanup failed");
    }
}

/// <summary>
/// A scoped service whose asynchronous disposal waits on a gate, which holds a service scope's disposal open.
/// </summary>
public sealed class ScopeDisposalGate : IAsyncDisposable
{
    /// <summary>
    /// Gets the gate that <see cref="DisposeAsync"/> waits on.
    /// </summary>
    public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
        => await Gate.Task;
}

/// <summary>
/// A <see cref="LifecycleRecordingViewModel"/> that depends on a <see cref="ScopeDisposalGate"/>, so the scope that created
/// it disposes the ViewModel first and then waits on the gate.
/// </summary>
/// <param name="scopeDisposalGate">The gate that holds the scope's disposal open.</param>
public sealed class ScopeGatedLifecycleViewModel(ScopeDisposalGate scopeDisposalGate) : LifecycleRecordingViewModel
{
    /// <summary>
    /// Gets the gate that holds the scope's disposal open.
    /// </summary>
    public ScopeDisposalGate ScopeDisposalGate { get; } = scopeDisposalGate;
}