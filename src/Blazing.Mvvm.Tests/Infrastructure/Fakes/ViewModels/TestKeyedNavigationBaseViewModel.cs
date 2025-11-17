using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Web;

namespace Blazing.Mvvm.Tests.Infrastructure.Fakes;

/// <summary>
/// Provides a base class for test view models supporting keyed navigation in Blazor MVVM tests.
/// Handles navigation, query string parsing, and exposes commands for navigation actions.
/// </summary>
public abstract partial class TestKeyedNavigationBaseViewModel : ViewModelBase, ITestKeyedNavigationViewModel
{
    /// <summary>
    /// The navigation manager for MVVM-based navigation.
    /// </summary>
    internal readonly IMvvmNavigationManager MvvmNavigationManager;
    /// <summary>
    /// The Blazor navigation manager.
    /// </summary>
    internal readonly NavigationManager NavigationManager;

    private RelayCommand? _keyedTestNavigateCommand;
    private bool isDisposed;

    /// <summary>
    /// Backing field for the keyed navigation command with parameters.
    /// </summary>
    internal RelayCommand<string>? KeyedTestNavigateCommandWithParamsImpl;

    /// <summary>
    /// Gets or sets the query string from the current navigation URI.
    /// </summary>
    [ObservableProperty]
    private string _queryString = string.Empty;

    /// <summary>
    /// Gets or sets the test parameter from the query string. Nullable.
    /// </summary>
    [ObservableProperty]
    private string? _test;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestKeyedNavigationBaseViewModel"/> class.
    /// </summary>
    /// <param name="mvvmNavigationManager">The MVVM navigation manager.</param>
    /// <param name="navigationManager">The Blazor navigation manager.</param>
    protected TestKeyedNavigationBaseViewModel(IMvvmNavigationManager mvvmNavigationManager, NavigationManager navigationManager)
    {
        MvvmNavigationManager = mvvmNavigationManager;
        NavigationManager = navigationManager;
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    /// <summary>
    /// Gets or sets the echo parameter, populated by <see cref="MvvmComponentBase"/>.
    /// </summary>
    [ViewParameter]
    public string? Echo { get; set; } = string.Empty;

    /// <summary>
    /// Gets the command for keyed test navigation without parameters.
    /// </summary>
    public RelayCommand KeyedTestNavigateCommand
        => _keyedTestNavigateCommand ??= new RelayCommand(() => NavigateToKey());

    /// <summary>
    /// Gets the command for keyed test navigation with parameters.
    /// </summary>
    public virtual RelayCommand<string> KeyedTestNavigateCommandWithParams
        => KeyedTestNavigateCommandWithParamsImpl ??= new RelayCommand<string>(NavigateWithParams);

    /// <summary>
    /// Called when the view model is initialized. Processes the query string.
    /// </summary>
    public override void OnInitialized()
        => ProcessQueryString();

    /// <summary>
    /// Disposes the view model and detaches navigation event handlers.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources used by the view model.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose()"/>; otherwise, false.</param>
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

    /// <summary>
    /// Handles location changes and processes the query string.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The location changed event arguments.</param>
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => ProcessQueryString();

    /// <summary>
    /// Navigates to the keyed test view model.
    /// </summary>
    private void NavigateToKey()
    {
        MvvmNavigationManager.NavigateTo("TestKeyedNavigationViewModel");
    }
    
    /// <summary>
    /// Navigates to the keyed test view model with parameters.
    /// </summary>
    /// <param name="@params">The parameters to append to the navigation URI.</param>
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

    /// <summary>
    /// Processes the query string from the current navigation URI and updates properties.
    /// </summary>
    private void ProcessQueryString()
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        QueryString = uri.Query;
        
        // Simple query string parsing
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            Test = queryParams["test"];
        }
        else
        {
            Test = null;
        }
    }
}
