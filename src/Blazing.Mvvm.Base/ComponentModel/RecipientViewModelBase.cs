using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// A base class for a <c>ViewModel</c> that implements <see cref="ObservableRecipient"/> which provides support for receiving messages and access to the <see cref="CommunityToolkit.Mvvm.Messaging.IMessenger"/> type.
/// </summary>
public abstract class RecipientViewModelBase : ObservableRecipient, IViewModelBase
{
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
}
