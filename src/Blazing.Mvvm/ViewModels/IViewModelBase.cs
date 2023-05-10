using System.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

public interface IViewModelBase : INotifyPropertyChanged
{
    Task OnInitializedAsync();

    Task Loaded();
}