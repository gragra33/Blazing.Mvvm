using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// A base class for a <c>ViewModel</c> that implements <see cref="ObservableObject"/> which provides support for property change notification and access to <c>View</c>'s life-cycle methods.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IViewModelBase
{
    /// <inheritdoc/>
    public virtual void OnAfterRender(bool firstRender)
    { }

    /// <inheritdoc/>
    public virtual Task OnAfterRenderAsync(bool firstRender)
        => Task.CompletedTask;

    /// <inheritdoc/>
    public virtual void OnInitialized()
    { }

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
