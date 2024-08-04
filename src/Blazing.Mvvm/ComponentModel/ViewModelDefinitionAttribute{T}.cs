namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// A generic attribute definition for a ViewModel.
/// </summary>
/// <typeparam name="TViewModel">The type the <c>ViewModel</c> implements that should be used for DI registration.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ViewModelDefinitionAttribute<TViewModel> : ViewModelDefinitionAttribute, IViewModelGenericAttributeDefinition
    where TViewModel : IViewModelBase
{
    /// <summary>
    /// The service type used for DI registration.
    /// </summary>
    public Type ViewModelType => typeof(TViewModel);
}
