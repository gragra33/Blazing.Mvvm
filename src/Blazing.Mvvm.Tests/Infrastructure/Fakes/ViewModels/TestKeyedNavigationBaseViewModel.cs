using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Web;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

public abstract partial class TestKeyedNavigationBaseViewModel : ViewModelBase, ITestKeyedNavigationViewModel
{
    internal readonly IMvvmNavigationManager MvvmNavigationManager;
    internal readonly NavigationManager NavigationManager;

    private RelayCommand? _keyedTestNavigateCommand;
    private bool isDisposed;

    internal RelayCommand<string>? KeyedTestNavigateCommandWithParamsImpl;

    [ObservableProperty]
    private string _queryString = string.Empty;

    [ObservableProperty]
    private string? _test;  // Changed to nullable

    protected TestKeyedNavigationBaseViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    {
        MvvmNavigationManager = mvvmNavigationManager;
        NavigationManager = navigationManager;
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    [ViewParameter] // populated by MvvmComponentBase
    public string? Echo { get; set; } = string.Empty;

    public RelayCommand KeyedTestNavigateCommand
        => _keyedTestNavigateCommand ??= new RelayCommand(() => NavigateToKey());

    public virtual RelayCommand<string> KeyedTestNavigateCommandWithParams
        => KeyedTestNavigateCommandWithParamsImpl ??= new RelayCommand<string>(NavigateWithParams);

    public override void OnInitialized()
        => ProcessQueryString();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            NavigationManager.LocationChanged -= OnLocationChanged;
        }

        isDisposed = true;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => ProcessQueryString();

    private void NavigateToKey()
    {
        MvvmNavigationManager.NavigateTo("TestKeyedNavigationViewModel");
    }
    
    private void NavigateWithParams(string? @params)
    {
        if (string.IsNullOrWhiteSpace(@params))
        {
            MvvmNavigationManager.NavigateTo("TestKeyedNavigationViewModel");
            return;
        }

        // For keyed navigation with parameters, call the correct overload
        MvvmNavigationManager.NavigateTo("TestKeyedNavigationViewModel", @params);
    }

    private void ProcessQueryString()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        QueryString = uri.Query;
        
        // Simple query string parsing
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            Test = queryParams["test"]; // Return null if not found, not empty string
        }
        else
        {
            Test = null; // Return null instead of empty string
        }
    }
}
