using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.Mvvm.Sample.Server.ViewModels;

[ViewModelDefinition<ITestNavigationViewModel>]
public partial class TestNavigationViewModel : ViewModelBase, ITestNavigationViewModel, IDisposable
{
    public TestNavigationViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    {
        _mvvmNavigationManager = mvvmNavigationManager;
        _navigationManager = navigationManager;
        _navigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => ProcessQueryString();

    private readonly IMvvmNavigationManager _mvvmNavigationManager;
    private readonly NavigationManager _navigationManager;

    [ObservableProperty]
    private string? _queryString;

    [ObservableProperty]
    private string? _test;

    public override Task Loaded()
    {
        ProcessQueryString();
        return base.Loaded();
    }

#pragma warning disable CS0649
    private RelayCommand? _hexTranslateNavigateCommand;
#pragma warning restore CS0649

#pragma warning disable CS0649
    private RelayCommand<string>? _testNavigateCommand;
#pragma warning restore CS0649

    public string? Echo { get; set; } = "";

    public RelayCommand HexTranslateNavigateCommand
        => _hexTranslateNavigateCommand ?? new RelayCommand(() => Navigate<HexTranslateViewModel>());

    public RelayCommand<string> TestNavigateCommand
        => _testNavigateCommand ?? new RelayCommand<string>(Navigate<ITestNavigationViewModel>);

    private void Navigate<T>(string? @params = null) where T : IViewModelBase
        => _mvvmNavigationManager.NavigateTo<T>(@params);

    private void ProcessQueryString()
    {
        QueryString = _navigationManager.ToAbsoluteUri(_navigationManager.Uri).Query;
        _navigationManager.TryGetQueryString("test", out string temp);
        Test = temp;
    }

    public void Dispose()
        => _navigationManager.LocationChanged -= OnLocationChanged;
}
