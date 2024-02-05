using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ViewModelDefinitionAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
}
