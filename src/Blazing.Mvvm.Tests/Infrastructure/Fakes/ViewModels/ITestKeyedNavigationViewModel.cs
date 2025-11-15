using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

public interface ITestKeyedNavigationViewModel : IViewModelBase, IDisposable
{
    string QueryString { get; set; }
    string? Test { get; set; }  // Changed to nullable
    string? Echo { get; set; }
    RelayCommand KeyedTestNavigateCommand { get; }
    RelayCommand<string> KeyedTestNavigateCommandWithParams { get; }
}
