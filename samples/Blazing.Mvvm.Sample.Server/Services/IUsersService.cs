using Blazing.Mvvm.Sample.Server.Models;

namespace Blazing.Mvvm.Sample.Server.Services;

/// <summary>
/// Service interface for managing users.
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>A collection of all users.</returns>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Checks if a user exists.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <returns>True if the user exists; otherwise, false.</returns>
    Task<bool> UserExistsAsync(string userId);
}
