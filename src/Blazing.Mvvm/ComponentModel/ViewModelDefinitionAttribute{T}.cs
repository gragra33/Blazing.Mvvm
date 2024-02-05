using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ViewModelDefinitionAttribute<TViewModel> : Attribute, IViewModelDefinition
    where TViewModel : IViewModelBase
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;

#if NET8_0_OR_GREATER
    public string? Key { get; set; }
#endif
}
