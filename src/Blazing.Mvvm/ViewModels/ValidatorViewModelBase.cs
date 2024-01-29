using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.ComponentModel;

public abstract partial class ValidatorViewModelBase : ObservableValidator, IViewModelBase
{
    public virtual async Task OnInitializedAsync()
        => await Loaded().ConfigureAwait(true);

    protected virtual void NotifyStateChanged() => OnPropertyChanged((string?)null);

    [RelayCommand]
    public virtual async Task Loaded()
        => await Task.CompletedTask.ConfigureAwait(false);
    public virtual ValueTask DisposeAsync()
    {
#if DEBUG
        Console.WriteLine($"..Disposing: {GetType().FullName}");
        return ValueTask.CompletedTask;
#endif
    }
}