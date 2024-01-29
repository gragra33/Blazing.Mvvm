using System.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

public interface IViewModelBase : INotifyPropertyChanged, IAsyncDisposable
{
    Task OnInitializedAsync();

    Task Loaded();
}