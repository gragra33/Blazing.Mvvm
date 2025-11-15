namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Specifies metadata for a generic <c>ViewModel</c> to control its registration in the dependency injection container.
/// Supports configuration of service lifetime, optional keyed registration, and DI registration for a specific service type.
/// </summary>
/// <typeparam name="TViewModel">The type the <c>ViewModel</c> implements that should be used for DI registration. Must implement <see cref="IViewModelBase"/>.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ViewModelDefinitionAttribute<TViewModel> : ViewModelDefinitionAttribute, IViewModelGenericAttributeDefinition
    where TViewModel : IViewModelBase
{
    /// <summary>
    /// Gets the service type used for DI registration. This is the type specified by <typeparamref name="TViewModel"/>.
    /// </summary>
    public Type ViewModelType => typeof(TViewModel);
}
