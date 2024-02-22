namespace Blazing.Mvvm.ComponentModel;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class ViewModelDefinitionAttribute<TViewModel> : ViewModelDefinitionAttributeBase, IViewModelDefinition
    where TViewModel : IViewModelBase
{
    public Type ViewModelType => typeof(TViewModel);

#if NET8_0_OR_GREATER
    public string? Key { get; set; }
#endif
}
