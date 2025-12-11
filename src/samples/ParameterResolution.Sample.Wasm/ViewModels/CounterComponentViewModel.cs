using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class CounterComponentViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private int _counter;
}