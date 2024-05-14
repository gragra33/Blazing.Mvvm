using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewModelDefinitionAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

#if NET8_0_OR_GREATER
    public string? Key { get; set; }
#endif
}
