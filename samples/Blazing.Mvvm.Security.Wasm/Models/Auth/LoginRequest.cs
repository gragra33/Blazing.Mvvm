using System.ComponentModel.DataAnnotations;

namespace Blazing.Mvvm.Security.Wasm.Models.Auth;

/// <summary>
/// Represents a login request with user credentials and options.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the username for login.
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password for login.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the login should be persistent.
    /// </summary>
    public bool RememberMe { get; set; }
}
