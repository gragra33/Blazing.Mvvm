using Blazing.Mvvm.Sample.WebApp.Client.Models;

namespace Blazing.Mvvm.Sample.WebApp.Client.Data;

/// <summary>
/// Service interface for managing posts.
/// </summary>
public interface IPostsService
{
    /// <summary>
    /// Gets all posts for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of posts for the user.</returns>
    Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId);

    /// <summary>
    /// Gets a specific post by user ID and post ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="postId">The post ID.</param>
    /// <returns>The post if found; otherwise, null.</returns>
    Task<Post?> GetPostByIdAsync(string userId, string postId);

    /// <summary>
    /// Checks if a post exists for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="postId">The post ID.</param>
    /// <returns>True if the post exists; otherwise, false.</returns>
    Task<bool> PostExistsAsync(string userId, string postId);
}
