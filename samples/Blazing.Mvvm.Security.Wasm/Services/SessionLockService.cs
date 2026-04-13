using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Blazing.Mvvm.Security.Wasm.Services;

/// <summary>
/// Manages session lock state based on authentication and RememberMe status.
/// Monitors authentication state changes and updates lock navigation flag accordingly.
/// </summary>
public sealed class SessionLockService : ObservableObject, IAsyncDisposable
{
    private readonly AuthenticationStateProvider _authProvider;
    private readonly LocalStorageService _localStorageService;
    private readonly IJSRuntime _jsRuntime;
    private bool _isDisposed;
    private Action? _onStateChanged;

    private bool _shouldLockNavigation;
    /// <summary>
    /// Gets or sets a value indicating whether navigation should be locked.
    /// Navigation is locked when user is authenticated but RememberMe was not selected.
    /// </summary>
    public bool ShouldLockNavigation
    {
        get => _shouldLockNavigation;
        private set
        {
            if (SetProperty(ref _shouldLockNavigation, value))
            {
                // Notify subscribers that state has changed
                _onStateChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionLockService"/> class.
    /// </summary>
    /// <param name="authProvider">The authentication state provider.</param>
    /// <param name="localStorageService">The local storage service.</param>
    /// <param name="jsRuntime">The JavaScript runtime for interop.</param>
    public SessionLockService(AuthenticationStateProvider authProvider, LocalStorageService localStorageService, IJSRuntime jsRuntime)
    {
        _authProvider = authProvider;
        _localStorageService = localStorageService;
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initializes the service and subscribes to authentication state changes.
    /// </summary>
    /// <param name="onStateChanged">Callback to notify when ShouldLockNavigation changes.</param>
    public async Task InitializeAsync(Action? onStateChanged = null)
    {
        _onStateChanged = onStateChanged;

        // Initial check
        await UpdateShouldLockNavigationAsync();

        // Notify initial state
        _onStateChanged?.Invoke();

        // Subscribe to auth state changes
        _authProvider.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    /// <summary>
    /// Handles authentication state changes and updates the lock navigation flag.
    /// </summary>
    /// <param name="task">The authentication state task.</param>
    private async void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        await UpdateShouldLockNavigationAsync();
    }

    /// <summary>
    /// Updates the <see cref="ShouldLockNavigation"/> state based on current authentication and RememberMe state.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateShouldLockNavigationAsync()
    {
        try
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();

            // User is authenticated
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                // Lock navigation if user was NOT remembered (RememberMe not checked)
                var isRemembered = await _localStorageService.IsAuthStateStoredAsync();
                ShouldLockNavigation = !isRemembered;
            }
            else
            {
                // User not authenticated, don't lock navigation
                ShouldLockNavigation = false;
            }
        }
        catch
        {
            ShouldLockNavigation = false;
        }
    }

    /// <summary>
    /// Determines whether the current user was remembered (loaded from localStorage).
    /// Returns true if user is authenticated and RememberMe was used during login.
    /// </summary>
    private async Task<bool> IsRememberedAsync()
    {
        try
        {
            var storedUser = await _localStorageService.GetAuthStateAsync();
            return storedUser?.IsAuthenticated == true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Shows a confirmation dialog asking if the user wants to leave the site.
    /// </summary>
    /// <returns>True if user confirmed; false if user canceled.</returns>
    public async Task<bool> ConfirmLeavingSiteAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>(
                "confirm",
                "Leaving will log you out. Are you sure?");
        }
        catch
        {
            // If confirmation fails, allow navigation to be safe
            return true;
        }
    }

    /// <summary>
    /// Cleans up resources and unsubscribes from authentication state changes.
    /// </summary>
    /// <returns>A value task representing the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _authProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        await ValueTask.CompletedTask;
    }
}
