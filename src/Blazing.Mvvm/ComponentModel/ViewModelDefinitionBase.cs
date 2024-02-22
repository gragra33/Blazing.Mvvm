using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public abstract class ViewModelDefinitionAttributeBase : Attribute
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
}
