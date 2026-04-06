using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Provides a base class for ViewModels that implement <see cref="ObservableObject"/>, supporting property change notification and Blazor MVVM lifecycle methods.
/// Implements <see cref="IViewModelBase"/> for integration with Blazor Views.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
    /// </summary>
    protected ViewModelBase()
    {
    }

    /// <summary>
    /// Stores the set of <see cref="IAsyncRelayCommand"/> instances that have been subscribed to property change notifications.
    /// </summary>
    private readonly HashSet<IAsyncRelayCommand> _subscribedCommands = [];

    private bool _commandSubscriptionsInitialized;
    private bool IsDisposed;

    /// <summary>
    /// Ensures async command state change subscriptions are initialized after derived construction has completed.
    /// </summary>
    public void EnsureCommandSubscriptions()
    {
        if (IsDisposed || _commandSubscriptionsInitialized)
        {
            return;
        }

        SubscribeToCommandPropertyChanged();
        _commandSubscriptionsInitialized = true;
    }

    /// <summary>
    /// Invoked when the <c>View</c> has been rendered.
    /// </summary>
    /// <param name="firstRender">Set to <see langword="true"/> if this is the first time the view is rendered; otherwise, <see langword="false"/>.</param>
    public virtual void OnAfterRender(bool firstRender)
    { /* skip */ }

    /// <summary>
    /// Asynchronously invoked when the <c>View</c> has been rendered.
    /// </summary>
    /// <param name="firstRender">Set to <see langword="true"/> if this is the first time the view is rendered; otherwise, <see langword="false"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task OnAfterRenderAsync(bool firstRender)
        => Task.CompletedTask;

    /// <summary>
    /// Invoked when the <c>View</c> is initialized and has received its initial parameters.
    /// </summary>
    public virtual void OnInitialized()
    { /* skip */ }

    /// <summary>
    /// Asynchronously invoked when the <c>View</c> is initialized and has received its initial parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task OnInitializedAsync()
        => Task.CompletedTask;

    /// <summary>
    /// Invoked when the <c>View</c> has received parameters from its parent and parameter values have been set.
    /// </summary>
    public virtual void OnParametersSet()
    { /* skip */ }

    /// <summary>
    /// Asynchronously invoked when the <c>View</c> has received parameters from its parent and parameter values have been set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task OnParametersSetAsync()
        => Task.CompletedTask;

    /// <summary>
    /// Determines whether the <c>View</c> should render.
    /// </summary>
    /// <returns><c>true</c> if the <c>View</c> should render; otherwise, <c>false</c>.</returns>
    public virtual bool ShouldRender()
        => true;

    /// <summary>
    /// Notifies the <c>View</c> that its state has changed and a UI update may be required.
    /// </summary>
    public virtual void NotifyStateChanged()
        => OnPropertyChanged();

    /// <summary>
    /// Subscribes to property changes for all <see cref="IAsyncRelayCommand"/> properties on this instance.
    /// </summary>
    private void SubscribeToCommandPropertyChanged()
    {
        foreach (var prop in GetType().GetProperties())
        {
            if (typeof(IAsyncRelayCommand).IsAssignableFrom(prop.PropertyType))
            {
                var command = prop.GetValue(this);
                if (command is INotifyPropertyChanged notifyCommand && _subscribedCommands.Add((IAsyncRelayCommand)command))
                {
                    notifyCommand.PropertyChanged += CommandPropertyChanged;
                }
            }
        }
    }

    /// <summary>
    /// Handles property changes for subscribed <see cref="IAsyncRelayCommand"/> instances.
    /// </summary>
    /// <param name="sender">The command that raised the event.</param>
    /// <param name="e">The event data.</param>
    protected virtual void CommandPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AsyncRelayCommand.IsRunning))
        {
            NotifyStateChanged();
        }
    }
 
    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="RecipientViewModelBase"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            // Unsubscribing from all command property change notifications.
            foreach (var command in _subscribedCommands)
            {
                command.PropertyChanged -= CommandPropertyChanged;
            }
            _subscribedCommands.Clear();
            _commandSubscriptionsInitialized = false;
        }
        IsDisposed = true;
    }

    /// <summary>
    /// Disposes the ViewModel and releases resources.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
