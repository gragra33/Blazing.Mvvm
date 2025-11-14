using Blazing.Mvvm.ComponentModel;

namespace HybridSample.Avalonia.States;

public interface INavigation
{
    void NavigateTo(string page);
    void NavigateTo<TViewModel>() where TViewModel : IViewModelBase;
}
