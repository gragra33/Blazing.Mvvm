using System.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Represents a base interface for ViewModel classes in Blazing.Mvvm, supporting property change notification and lifecycle methods for Blazor Views.
/// </summary>
public interface IViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Invoked when the <c>View</c> has been rendered.
    /// </summary>
    /// <param name="firstRender">Set to <see langword="true"/> if this is the first time the view is rendered; otherwise, <see langword="false"/>.</param>
    void OnAfterRender(bool firstRender);

    /// <summary>
    /// Asynchronously invoked when the <c>View</c> has been rendered.
    /// </summary>
    /// <param name="firstRender">Set to <see langword="true"/> if this is the first time the view is rendered; otherwise, <see langword="false"/>.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task OnAfterRenderAsync(bool firstRender);

    /// <summary>
    /// Invoked when the <c>View</c> is initialized and has received its initial parameters.
    /// </summary>
    void OnInitialized();

    /// <summary>
    /// Asynchronously invoked when the <c>View</c> is initialized and has received its initial parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task OnInitializedAsync();

    /// <summary>
    /// Invoked when the <c>View</c> has received parameters from its parent and parameter values have been set.
    /// </summary>
    void OnParametersSet();

    /// <summary>
    /// Asynchronously invoked when the <c>View</c> has received parameters from its parent and parameter values have been set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task OnParametersSetAsync();

    /// <summary>
    /// Determines whether the <c>View</c> should render.
    /// </summary>
    /// <returns><c>true</c> if the <c>View</c> should render; otherwise, <c>false</c>.</returns>
    bool ShouldRender();

    /// <summary>
    /// Notifies the <c>View</c> that its state has changed and a UI update may be required.
    /// </summary>
    void NotifyStateChanged();
}
