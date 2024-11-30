using System.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// An interface that represents a <c>ViewModel</c>.
/// </summary>
public interface IViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// This method is invoked when the <c>View</c> has been rendered.
    /// </summary>
    /// <param name="firstRender">Set to <see langword="true"/> if this is the first time view is rendered otherwise <see langword="false"/>.</param>
    void OnAfterRender(bool firstRender);

    /// <summary>
    /// This method is invoked when the <c>View</c> has been rendered.
    /// </summary>
    /// <param name="firstRender">Set to <see langword="true"/> if this is the first time view is rendered otherwise <see langword="false"/>.</param>
    /// <returns>A <see cref="Task"/> representing an asynchronous operation.</returns>
    Task OnAfterRenderAsync(bool firstRender);

    /// <summary>
    /// This method is invoked when the <c>View</c> is initialized and has received its initial parameters.
    /// </summary>
    void OnInitialized();

    /// <summary>
    /// This method is invoked when the <c>View</c> is initialized and has received its initial parameters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing an asynchronous operation.</returns>
    Task OnInitializedAsync();

    /// <summary>
    /// This method is invoked when the <c>View</c> has received parameters from its parent and parameters values have been set.
    /// </summary>
    void OnParametersSet();

    /// <summary>
    /// This method is invoked when the <c>View</c> has received parameters from its parent and parameters values have been set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing an asynchronous operation.</returns>
    Task OnParametersSetAsync();

    /// <summary>
    /// This method returns a <see cref="bool"/> indicating whether the <c>View</c> should render.
    /// </summary>
    /// <returns>A <see cref="bool">flag</see> indicating whether the <c>View</c> should render.</returns>
    bool ShouldRender();

    /// <summary>
    /// Notifies the <c>View</c> that its state has changed.
    /// </summary>
    void NotifyStateChanged();
}
