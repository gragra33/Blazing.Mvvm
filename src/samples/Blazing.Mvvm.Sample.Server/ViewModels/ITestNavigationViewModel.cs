using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

public interface ITestNavigationViewModel : IViewModelBase, IDisposable
{
    string QueryString { get; set; }

    string Test { get; set; }

    string? Echo { get; set; }

    RelayCommand HexTranslateNavigateCommand { get; }

    RelayCommand<string> TestNavigateCommand { get; }
}
