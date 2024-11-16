using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

[ViewModelDefinition<ITestKeyedNavigationViewModel>(Key = nameof(TestKeyedNavigationViewModel))]
public sealed class TestKeyedNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    : TestNavigationBaseViewModel(mvvmNavigationManager, navigationManager), ITestKeyedNavigationViewModel
{
    public override RelayCommand<string> TestNavigateCommand
        => TestNavigateCommandImpl ??= new RelayCommand<string>(s => Navigate(nameof(TestKeyedNavigationViewModel), s));

    private void Navigate(string key, string? @params = null)
    {
        if (string.IsNullOrWhiteSpace(@params))
        {
            MvvmNavigationManager.NavigateTo(key);
            return;
        }

        MvvmNavigationManager.NavigateTo(key, @params);
    }
}

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