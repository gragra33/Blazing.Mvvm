namespace Blazing.Mvvm.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ViewModelDefinitionAttribute<TViewModel> : ViewModelDefinitionAttribute, IViewModelGenericAttributeDefinition
    where TViewModel : IViewModelBase
{
    public Type ViewModelType => typeof(TViewModel);
}
