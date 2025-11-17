using CommunityToolkit.Mvvm.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Provides a base class for ViewModels that implement <see cref="ObservableObject"/>, supporting property change notification and Blazor MVVM lifecycle methods.
/// Implements <see cref="IViewModelBase"/> for integration with Blazor Views.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IViewModelBase
{
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
}
