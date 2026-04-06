using System.Diagnostics.Metrics;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ParameterResolution.Sample.Wasm.ViewModels;

[ViewModelDefinition]
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