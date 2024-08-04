using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// A base class for a <c>ViewModel</c> that implements <see cref="ObservableValidator"/> which provides support for validation.
/// </summary>
public abstract class ValidatorViewModelBase : ObservableValidator, IViewModelBase
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
