using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// An attribute that defines a <c>ViewModel</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewModelDefinitionAttribute : Attribute
{
    /// <summary>
    /// Defines the lifetime of the <c>ViewModel</c>. The default is <see cref="ServiceLifetime.Transient"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

    /// <summary>
    /// Defines the key of the <c>ViewModel</c>. When set the <c>ViewModel</c> is registered as a keyed service.
    /// </summary>
    public object? Key { get; set; }
}
