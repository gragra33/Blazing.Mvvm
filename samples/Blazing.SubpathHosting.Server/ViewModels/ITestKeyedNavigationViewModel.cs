using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Blazing.SubpathHosting.Server.ViewModels;

public interface ITestKeyedNavigationViewModel : IViewModelBase
{
    RelayCommand<string> TestNavigateCommand { get; }

    /// <inheritdoc cref="TestNavigationBaseViewModel._queryString"/>
    string? QueryString { get; set; }

    /// <inheritdoc cref="TestNavigationBaseViewModel._test"/>
    string? Test { get; set; }

    // populated by MvvmComponentBase
    string? Echo { get; set; }

    RelayCommand HexTranslateNavigateCommand { get; }
}