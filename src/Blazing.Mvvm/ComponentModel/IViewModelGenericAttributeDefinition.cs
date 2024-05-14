using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

internal interface IViewModelGenericAttributeDefinition
{
    ServiceLifetime Lifetime { get; set; }

    Type ViewModelType { get; }

#if NET8_0_OR_GREATER
    string? Key { get; set; }
#endif
}