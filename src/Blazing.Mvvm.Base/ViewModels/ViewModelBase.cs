using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.ComponentModel;

public abstract partial class ViewModelBase : ObservableObject, IViewModelBase
{
    public virtual async Task OnInitializedAsync()
        => await Loaded().ConfigureAwait(true);

    protected virtual void NotifyStateChanged() => OnPropertyChanged((string?)null);

    [RelayCommand]
    public virtual async Task Loaded()
        => await Task.CompletedTask.ConfigureAwait(false);
}