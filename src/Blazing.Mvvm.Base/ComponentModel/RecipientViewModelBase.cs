using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Provides a base class for ViewModels that implement <see cref="ObservableRecipient"/>, supporting message reception and access to <see cref="IMessenger"/>.
/// Implements <see cref="IViewModelBase"/> for Blazor MVVM lifecycle integration and <see cref="IDisposable"/> for resource cleanup.
/// </summary>
public abstract class RecipientViewModelBase : ObservableRecipient, IViewModelBase, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipientViewModelBase"/> class.
    /// </summary>
    protected RecipientViewModelBase()
    { /* skip */ }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipientViewModelBase"/> class with the specified <see cref="IMessenger"/>.
    /// </summary>
    /// <param name="messenger">The messenger instance for message delivery.</param>
    protected RecipientViewModelBase(IMessenger messenger)
        : base(messenger)
    { /* skip */ }

    private bool IsDisposed;

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
        }
        IsDisposed = true;
    }

    /// <summary>
    /// Disposes the ViewModel and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
