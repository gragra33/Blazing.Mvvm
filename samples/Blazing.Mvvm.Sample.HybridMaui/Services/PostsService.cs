using Blazing.Mvvm.Sample.HybridMaui.Models;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Sample.HybridMaui.Services;

/// <summary>
/// Mock implementation of the posts service.
/// </summary>
public class PostsService : IPostsService
{
    private readonly ILogger<PostsService> _logger;
    private static readonly List<Post> _posts =
    [
        // Alice's posts (userId: 1)
        new Post { Id = "101", UserId = "1", Title = "Getting Started with Blazor", Content = "Blazor is a framework for building interactive web UIs using C#...", Preview = "Learn the basics of Blazor..." },
        new Post { Id = "102", UserId = "1", Title = "MVVM Pattern in Blazor", Content = "The MVVM pattern helps separate concerns in your application...", Preview = "Understand MVVM in Blazor..." },
        new Post { Id = "103", UserId = "1", Title = "Advanced Blazor Components", Content = "Creating reusable components is key to building maintainable apps...", Preview = "Master component design..." },
        
        // Bob's posts (userId: 2)
        new Post { Id = "201", UserId = "2", Title = "Introduction to .NET MAUI", Content = ".NET MAUI is the evolution of Xamarin.Forms...", Preview = "Start with .NET MAUI..." },
        new Post { Id = "202", UserId = "2", Title = "Cross-Platform Development", Content = "Building apps for multiple platforms with a single codebase...", Preview = "Go cross-platform..." },
        
        // Charlie's posts (userId: 3)
        new Post { Id = "301", UserId = "3", Title = "Clean Code Principles", Content = "Writing clean, maintainable code is essential...", Preview = "Write cleaner code..." },
        new Post { Id = "302", UserId = "3", Title = "SOLID Principles Explained", Content = "SOLID principles help create better software architecture...", Preview = "Understand SOLID..." },
        new Post { Id = "303", UserId = "3", Title = "Unit Testing Best Practices", Content = "Testing is crucial for reliable software...", Preview = "Master unit testing..." },
        
        // Deep Thought's posts (userId: 42)
        new Post { Id = "789", UserId = "42", Title = "The Answer to Everything", Content = "After 7.5 million years of computation, the answer is...", Preview = "The ultimate answer..." },
        new Post { Id = "421", UserId = "42", Title = "Life, Universe, and Everything", Content = "Deep questions require deep thought...", Preview = "Philosophical musings..." },
        
        // Test User's posts (userId: 2503)
        new Post { Id = "101", UserId = "2503", Title = "First Test Post", Content = "This is a test post for demonstration purposes...", Preview = "Testing the system..." },
        new Post { Id = "102", UserId = "2503", Title = "Second Test Post", Content = "Another test post to show multiple posts...", Preview = "More testing..." },
        new Post { Id = "789", UserId = "2503", Title = "Popular Test Post", Content = "This is a very popular test post...", Preview = "A popular post..." }
    ];

    public PostsService(ILogger<PostsService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
    {
        _logger.LogDebug("Getting posts for user: {UserId}", userId);
        var posts = _posts.Where(p => p.UserId == userId);
        return Task.FromResult(posts);
    }

    public Task<Post?> GetPostByIdAsync(string userId, string postId)
    {
        _logger.LogDebug("Getting post {PostId} for user {UserId}", postId, userId);
        var post = _posts.FirstOrDefault(p => p.UserId == userId && p.Id == postId);
        
        if (post == null)
        {
            _logger.LogWarning("Post {PostId} not found for user {UserId}", postId, userId);
        }
        
        return Task.FromResult(post);
    }

    public Task<bool> PostExistsAsync(string userId, string postId)
    {
        _logger.LogDebug("Checking if post {PostId} exists for user {UserId}", postId, userId);
        return Task.FromResult(_posts.Any(p => p.UserId == userId && p.Id == postId));
    }
}
