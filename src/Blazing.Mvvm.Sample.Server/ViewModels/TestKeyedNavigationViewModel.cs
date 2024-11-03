using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

[ViewModelDefinition(Key = nameof(TestKeyedNavigationViewModel), Lifetime = ServiceLifetime.Scoped)]
public sealed class TestKeyedNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    : TestNavigationBaseViewModel(mvvmNavigationManager, navigationManager)
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