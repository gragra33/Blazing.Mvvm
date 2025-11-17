using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Specifies metadata for a <c>ViewModel</c> to control its registration in the dependency injection container.
/// Supports configuration of service lifetime and optional keyed registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewModelDefinitionAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the lifetime of the <c>ViewModel</c> in the dependency injection container. The default is <see cref="ServiceLifetime.Transient"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Gets or sets the key for the <c>ViewModel</c>. When set, the <c>ViewModel</c> is registered as a keyed service.
    /// </summary>
    public object? Key { get; set; }
}
