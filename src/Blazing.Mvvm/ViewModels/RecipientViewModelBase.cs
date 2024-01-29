using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.ComponentModel;

public abstract partial class RecipientViewModelBase : ObservableRecipient, IViewModelBase
{
    public virtual async Task OnInitializedAsync()
    {
        await Loaded().ConfigureAwait(true);
    }

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

public abstract partial class RecipientViewModelBase<TMessage> : RecipientViewModelBase, IRecipient<TMessage> 
    where TMessage : class
{
    public abstract void Receive(TMessage message);
}