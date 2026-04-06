using HybridSample.Core.Models;
using Refit;

namespace HybridSample.Core.Services;

/// <summary>
/// An interface for a simple Reddit service.
/// </summary>
public interface IRedditService
{
    /// <summary>
    /// Get a list of posts from a given subreddit
    /// </summary>
    /// <param name="subreddit">The subreddit name.</param>
    [Headers("User-Agent: Awesome Blazor CommunityToolkit.Mvvm Sample App")]
    [Get("/r/{subreddit}/hot.json")]
    Task<PostsQueryResponse> GetSubredditPostsAsync(string subreddit);
}
