using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Mvvm.ComponentModel;

public interface IViewModelGenericAttributeDefinition
{
    ServiceLifetime Lifetime { get; set; }

    Type ViewModelType { get; }

    object? Key { get; set; }
}
