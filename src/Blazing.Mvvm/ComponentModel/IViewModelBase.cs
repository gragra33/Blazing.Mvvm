using System.ComponentModel;

namespace Blazing.Mvvm.ComponentModel;

public interface IViewModelBase : INotifyPropertyChanged, IAsyncDisposable, IDisposable
{
    Task OnInitializedAsync();

    Task Loaded();
}
