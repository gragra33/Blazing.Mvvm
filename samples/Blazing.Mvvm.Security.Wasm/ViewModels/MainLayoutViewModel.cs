using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Security.Wasm.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;

namespace Blazing.Mvvm.Security.Wasm.ViewModels;

/// <summary>
/// ViewModel for the main layout. Coordinates session lock state and navigation events.
/// Enforces authentication by redirecting unauthenticated users to the login page.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class MainLayoutViewModel : ViewModelBase
{
    private readonly NavigationManager _navigationManager;
    private readonly SessionLockService _sessionLockService;
    private readonly AuthenticationStateProvider _authStateProvider;

    [ObservableProperty]
    private int _counter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainLayoutViewModel"/> class.
    /// </summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="sessionLockService">The session lock service.</param>
    /// <param name="authStateProvider">The authentication state provider.</param>
    public MainLayoutViewModel(NavigationManager navigationManager, SessionLockService sessionLockService, AuthenticationStateProvider authStateProvider)
    {
        _navigationManager = navigationManager;
        _sessionLockService = sessionLockService;
        _authStateProvider = authStateProvider;
        _navigationManager.LocationChanged += OnLocationChanged;
        _authStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
    }

    /// <summary>
    /// Gets the session lock service for binding to NavigationLock.
    /// </summary>
    public SessionLockService SessionLockService => _sessionLockService;

    /// <summary>
    /// Initializes the ViewModel and starts monitoring session lock state.
    /// Redirects to login if the user is not authenticated.
    /// </summary>
    public override async Task OnInitializedAsync()
    {
        await _sessionLockService.InitializeAsync(() => OnPropertyChanged(nameof(SessionLockService)));
        await RedirectIfUnauthenticatedAsync();
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Handles internal navigation events (if required).
    /// </summary>
    public Task OnBeforeInternalNavigationAsync(LocationChangingContext context)
    {
        // if required, add handler here...
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the ViewModel and unsubscribes from navigation events.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _navigationManager.LocationChanged -= OnLocationChanged;
            _authStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
            _sessionLockService.DisposeAsync().GetAwaiter().GetResult();
        }
        base.Dispose(disposing);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        => Counter++;

    private async void OnAuthStateChanged(Task<AuthenticationState> authStateTask)
    {
        await RedirectIfUnauthenticatedAsync(authStateTask);
    }

    private async Task RedirectIfUnauthenticatedAsync(Task<AuthenticationState>? authStateTask = null)
    {
        var authState = await (authStateTask ?? _authStateProvider.GetAuthenticationStateAsync());
        if (authState.User.Identity?.IsAuthenticated != true)
        {
            var returnUrl = Uri.EscapeDataString(_navigationManager.ToBaseRelativePath(_navigationManager.Uri));
            _navigationManager.NavigateTo($"login?returnUrl={returnUrl}");
        }
    }
}
