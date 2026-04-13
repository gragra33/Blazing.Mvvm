using System.Security.Claims;
using Blazing.Mvvm.Security.Wasm.Models.Auth;
using Blazing.Mvvm.Security.Wasm.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazing.Mvvm.Security.Wasm.Providers;

/// <summary>
/// Provides authentication state using localStorage for persistence.
/// Implements Blazor WebAssembly authentication patterns with support for login, logout, and claim restoration.
/// </summary>
public class LocalAuthStateProvider(InMemoryAuthService authService, LocalStorageService localStorageService)
    : AuthenticationStateProvider
{
    private CurrentUser? _currentUser;
    private bool _isLoggingOut;

    /// <summary>
    /// Gets the current <see cref="AuthenticationState"/> for the user, restoring from localStorage if available.
    /// </summary>
    /// <returns>The current <see cref="AuthenticationState"/>.</returns>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        try
        {
            var userInfo = await GetCurrentUserAsync();
            
            if (userInfo?.IsAuthenticated == true)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, userInfo.UserName),
                    new(ClaimTypes.Email, userInfo.Email)
                };

                // Create a snapshot of claims to avoid collection modification during enumeration
                var claimSnapshot = userInfo.Claims.ToList();
                claims.AddRange(claimSnapshot.Select(c => new Claim(c.Key, c.Value)));
                
                identity = new ClaimsIdentity(claims, "LocalStorage");
            }
        }
        catch (Exception)
        {
            // Log exception if needed, but swallow it to return unauthenticated state
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    /// <summary>
    /// Gets the current user, restoring from localStorage if available and not logging out.
    /// </summary>
    /// <returns>The current <see cref="CurrentUser"/> or null if not authenticated.</returns>
    private async Task<CurrentUser?> GetCurrentUserAsync()
    {
        // NEVER restore from localStorage if we're logging out
        if (_isLoggingOut)
        {
            return new CurrentUser { IsAuthenticated = false };
        }
        
        if (_currentUser?.IsAuthenticated == true)
        {
            return _currentUser;
        }

        // Try to restore from localStorage (only if not logging out)
        _currentUser = await localStorageService.GetAuthStateAsync();
        
        if (_currentUser?.IsAuthenticated == true)
        {
            return _currentUser;
        }

        _currentUser = authService.GetCurrentUser();
        return _currentUser;
    }

    /// <summary>
    /// Attempts to log in the user with the specified login request.
    /// Persists authentication state to localStorage if RememberMe is set.
    /// </summary>
    /// <param name="loginRequest">The login request containing credentials and RememberMe flag.</param>
    public async Task LoginAsync(LoginRequest loginRequest)
    {
        var isSuccess = authService.Login(loginRequest);

        if (isSuccess)
        {
            var user = authService.GetCurrentUser();
            _currentUser = user;

            // Save to localStorage if RememberMe is checked, otherwise clear it
            if (loginRequest.RememberMe && user.IsAuthenticated)
            {
                await localStorageService.SetAuthStateAsync(user);
            }
            else
            {
                // Clear localStorage when RememberMe is false to prevent old sessions from persisting
                await localStorageService.RemoveAuthStateAsync();
            }

            // Create the AuthenticationState FIRST, then notify
            var authState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }
    }

    /// <summary>
    /// Logs out the current user, clearing authentication state from memory and localStorage.
    /// </summary>
    public async Task LogoutAsync()
    {
        // Set flag to prevent localStorage restoration during logout
        _isLoggingOut = true;
        
        // Clear from localStorage FIRST (before anything else)
        await localStorageService.RemoveAuthStateAsync();

        // Clear in-memory state
        _currentUser = null;
        authService.Logout();

        // Create the AuthenticationState FIRST, then notify
        var authState = await GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(authState));

        // Reset flag after state change notification completes
        _isLoggingOut = false;
    }
}
