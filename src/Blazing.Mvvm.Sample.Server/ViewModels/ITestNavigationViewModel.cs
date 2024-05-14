using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

public interface ITestNavigationViewModel : IViewModelBase, IDisposable
{
    public string QueryString { get; set; }

    public string Test { get; set; }

    string? Echo { get; set; }

    RelayCommand HexTranslateNavigateCommand { get; }

    RelayCommand<string> TestNavigateCommand { get; }

}