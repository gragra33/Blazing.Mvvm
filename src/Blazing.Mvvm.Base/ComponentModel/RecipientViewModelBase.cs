using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// A base class for a <c>ViewModel</c> that implements <see cref="ObservableRecipient"/> which provides support for receiving messages and access to the <see cref="CommunityToolkit.Mvvm.Messaging.IMessenger"/> type.
/// </summary>
public abstract class RecipientViewModelBase : ObservableRecipient, IViewModelBase, IDisposable
{

     /// <inheritdoc/>
     protected RecipientViewModelBase()
        : base()
     {}
    
     /// <inheritdoc/>
     protected RecipientViewModelBase(IMessenger messenger)
        : base(messenger)
    {}
    private bool IsDisposed;

    /// <inheritdoc/>
    public virtual void OnAfterRender(bool firstRender)
    { }

    /// <inheritdoc/>
    public virtual Task OnAfterRenderAsync(bool firstRender)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual void OnInitialized()
    {
        // Setting this to true ensuring the OnActivated() method will be invoked, which will register all necessary message handlers for this recipient.
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
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
