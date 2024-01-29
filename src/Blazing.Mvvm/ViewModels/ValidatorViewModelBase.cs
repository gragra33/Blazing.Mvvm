using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.ComponentModel;

public abstract partial class ValidatorViewModelBase : ObservableValidator, IViewModelBase
{
    private bool _disposed;

    public virtual async Task OnInitializedAsync()
        => await Loaded().ConfigureAwait(true);

    protected virtual void NotifyStateChanged() => OnPropertyChanged((string?)null);

    [RelayCommand]
    public virtual async Task Loaded()
        => await Task.CompletedTask.ConfigureAwait(false);

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

#if DEBUG
        Console.WriteLine($"..Disposing: {GetType().FullName}");
#endif
        _disposed = true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public virtual ValueTask DisposeAsync()
    {
        if (_disposed)
            return ValueTask.CompletedTask;

        Dispose();
        return ValueTask.CompletedTask;
    }

    #endregion
}