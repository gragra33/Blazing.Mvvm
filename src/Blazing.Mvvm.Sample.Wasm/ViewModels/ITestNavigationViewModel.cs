using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

public interface ITestNavigationViewModel : IViewModelBase
{
    public string QueryString { get; set; }

    public string Test { get; set; }

    string? Echo { get; set; }
    
    RelayCommand HexTranslateNavigateCommand { get; }
    
    RelayCommand<string> TestNavigateCommand { get; }

}