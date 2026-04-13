using Blazing.Mvvm.Security.Wasm.Models.Auth;

namespace Blazing.Mvvm.Security.Wasm.Services;

/// <summary>
/// Provides an in-memory authentication service for demonstration and testing purposes.
/// </summary>
public class InMemoryAuthService
{
    private CurrentUser? _currentUser;

    // In-memory user store: Username -> (Password, Email)
    private readonly Dictionary<string, (string Password, string Email)> _validUsers = new()
    {
        ["admin"] = ("Password123!", "admin@example.com"),
        ["user"] = ("User123!", "user@example.com"),
    };


    /// <summary>
    /// Gets the current authenticated user, or a new unauthenticated user if none is set.
    /// </summary>
    /// <returns>The current <see cref="CurrentUser"/> or a new unauthenticated user.</returns>
    public CurrentUser GetCurrentUser()
        => _currentUser ??= new CurrentUser();

    /// <summary>
    /// Attempts to log in a user with the specified login request.
    /// </summary>
    /// <param name="loginRequest">The login request containing username and password.</param>
    /// <returns><c>true</c> if login is successful; otherwise, <c>false</c>.</returns>
    public bool Login(LoginRequest loginRequest)
    {
        var matchedUser = _validUsers
            .Where(x =>
                x.Key.Equals(loginRequest.UserName, StringComparison.OrdinalIgnoreCase) &&
                x.Value.Password.Equals(loginRequest.Password))
            .Select(x =>
            {
                var claims = new Dictionary<string, string>
                {
                    ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] = Guid.NewGuid().ToString(),
                    ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] = x.Key,
                    ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email"] = x.Value.Email,
                    ["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] = x.Key.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User",
                };
                
                return new CurrentUser
                {
                    UserName = x.Key,
                    Email = x.Value.Email,
                    IsAuthenticated = true,
                    Claims = claims
                };
            })
            .FirstOrDefault();

        if (matchedUser != null)
        {
            _currentUser = matchedUser;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Logs out the current user and clears authentication state.
    /// </summary>
    public void Logout()
    {
        _currentUser = null;
    }
}
