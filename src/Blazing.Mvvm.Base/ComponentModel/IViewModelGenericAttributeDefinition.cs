using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Defines the contract for generic attribute-based ViewModel registration, supporting service lifetime, type, and optional key for keyed registration.
/// </summary>
public interface IViewModelGenericAttributeDefinition
{
    /// <summary>
    /// Gets or sets the service lifetime for the ViewModel registration (e.g., Singleton, Scoped, Transient).
    /// </summary>
    ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Gets the type of the ViewModel to be registered.
    /// </summary>
    Type ViewModelType { get; }

    /// <summary>
    /// Gets or sets the key for keyed ViewModel registration, or <c>null</c> for non-keyed registration.
    /// </summary>
    object? Key { get; set; }
}
