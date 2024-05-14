using System.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

public interface IViewModelBase : INotifyPropertyChanged
{
    Task Loaded();

    void OnAfterRender(bool firstRender);

    Task OnAfterRenderAsync(bool firstRender);

    void OnInitialized();

    Task OnInitializedAsync();

    void OnParametersSet();

    Task OnParametersSetAsync();

    bool ShouldRender();

    void NotifyStateChanged();
}
