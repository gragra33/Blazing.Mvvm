using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ParameterResolution.Sample.Wasm.ViewModels;

public sealed partial class ParameterDemoViewModel : ViewModelBase
{
    [ObservableProperty]
    [property: ViewParameter]
    private string _title = default!;

    [ObservableProperty]
    [property: ViewParameter("Count")]
    private int _counter;

    [ViewParameter]
    public string? Content { get; set; }

    [ObservableProperty]
    private string _displayMessage = string.Empty;

    public override void OnParametersSet()
    {
        UpdateDisplayMessage();
    }

    [RelayCommand]
    private void IncrementCounter()
    {
        Counter++;
        UpdateDisplayMessage();
    }

    [RelayCommand]
    private void DecrementCounter()
    {
        Counter--;
        UpdateDisplayMessage();
    }

    private void UpdateDisplayMessage()
    {
        DisplayMessage = $"Received Parameters:\n" +
                        $"Title: {Title}\n" +
                        $"Counter (via Count param): {Counter}\n" +
                        $"Content: {Content ?? "(null)"}";
    }
}
