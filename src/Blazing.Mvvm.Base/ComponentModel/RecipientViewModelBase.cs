using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Provides a base class for ViewModels that implement <see cref="ObservableRecipient"/>, supporting message reception and access to <see cref="IMessenger"/>.
/// Implements <see cref="IViewModelBase"/> for Blazor MVVM lifecycle integration and <see cref="IDisposable"/> for resource cleanup.
/// </summary>
public abstract class RecipientViewModelBase : ObservableRecipient, IViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipientViewModelBase"/> class.
    /// </summary>
    protected RecipientViewModelBase()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipientViewModelBase"/> class with the specified <see cref="IMessenger"/>.
    /// </summary>
    /// <param name="messenger">The messenger instance for message delivery.</param>
    protected RecipientViewModelBase(IMessenger messenger)
        : base(messenger)
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

    /// <inheritdoc/>
    public virtual void OnAfterRender(bool firstRender)
    { /* skip */ }

    /// <inheritdoc/>
    public virtual Task OnAfterRenderAsync(bool firstRender)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual void OnInitialized()
    {
        // Setting this to true ensures the OnActivated() method will be invoked, registering all necessary message handlers for this recipient.
        IsActive = true;
    }

    /// <inheritdoc/>
    public virtual Task OnInitializedAsync()
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual void OnParametersSet()
    { }

    /// <inheritdoc/>
    public virtual Task OnParametersSetAsync()
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual bool ShouldRender()
        => true;

    /// <inheritdoc/>
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
            ObservableRecipient observableRecipient = this;
            observableRecipient.IsActive = false;

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
