using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

// Test ViewModels for navigation tests
public interface ITestNavigationViewModel : IViewModelBase, IDisposable
{
    string QueryString { get; set; }
    string? Test { get; set; }  // Changed to nullable
    string? Echo { get; set; }
    RelayCommand HexTranslateNavigateCommand { get; }
    RelayCommand<string> TestNavigateCommand { get; }
}
