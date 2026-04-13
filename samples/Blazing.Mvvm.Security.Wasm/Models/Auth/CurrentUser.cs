namespace Blazing.Mvvm.Security.Wasm.Models.Auth;

/// <summary>
/// Represents the current authenticated user and their claims.
/// </summary>
public class CurrentUser
{
    /// <summary>
    /// Gets or sets a value indicating whether the user is authenticated.
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Gets or sets the user's display name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the claims associated with the user.
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = [];
}
