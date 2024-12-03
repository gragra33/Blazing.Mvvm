using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Defines a generic attribute for ViewModel registration.
/// </summary>
public interface IViewModelGenericAttributeDefinition
{
    /// <summary>
    /// Gets or sets the service lifetime for the ViewModel.
    /// </summary>
    ServiceLifetime Lifetime { get; set; }

    /// <summary>
    /// Gets the type of the ViewModel.
    /// </summary>
    Type ViewModelType { get; }

    /// <summary>
    /// Gets or sets the key for the ViewModel registration.
    /// </summary>
    object? Key { get; set; }
}
