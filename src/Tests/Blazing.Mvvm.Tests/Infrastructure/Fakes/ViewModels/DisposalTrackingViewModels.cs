using Blazing.Mvvm.ComponentModel;
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
