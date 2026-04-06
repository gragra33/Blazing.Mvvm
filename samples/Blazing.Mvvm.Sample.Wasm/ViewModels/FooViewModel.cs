using System.ComponentModel;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

// sample ViewModel showing Blazor Analyzers in action!
public class FooViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _name;
    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}