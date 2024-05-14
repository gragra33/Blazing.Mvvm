using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.ComponentModel;

public abstract partial class ValidatorViewModelBase : ObservableValidator, IViewModelBase
{
    [RelayCommand]
    public virtual async Task Loaded()
        => await Task.CompletedTask.ConfigureAwait(false);

    public virtual void OnAfterRender(bool firstRender)
    { }

    public virtual Task OnAfterRenderAsync(bool firstRender)
        => Task.CompletedTask;

    public virtual void OnInitialized()
    { }

    public virtual async Task OnInitializedAsync()
        => await Loaded().ConfigureAwait(true);

    public virtual void OnParametersSet()
    { }

    public virtual Task OnParametersSetAsync()
        => Task.CompletedTask;

    public virtual bool ShouldRender()
        => true;

    public virtual void NotifyStateChanged()
        => OnPropertyChanged();
}