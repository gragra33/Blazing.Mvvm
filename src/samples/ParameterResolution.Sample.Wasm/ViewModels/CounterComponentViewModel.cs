using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class CounterComponentViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter;

    [RelayCommand]
    private void IncrementCounter()
    {
        Counter++;
    }

    [RelayCommand]
    private void DecrementCounter()
    {
        Counter--;
    }
}