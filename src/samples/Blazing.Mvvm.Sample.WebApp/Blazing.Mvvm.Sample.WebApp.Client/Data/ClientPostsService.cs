using System.Net.Http.Json;
using Blazing.Mvvm.Sample.WebApp.Client.Models;

namespace Blazing.Mvvm.Sample.WebApp.Client.Data;

public class ClientPostsService : IPostsService
{
    private readonly HttpClient _httpClient;

    public ClientPostsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
    {
        var posts = await _httpClient.GetFromJsonAsync<IEnumerable<Post>>($"/api/users/{userId}/posts");
        return posts ?? [];
    }

    public Task<Post?> GetPostByIdAsync(string userId, string postId)
        => _httpClient.GetFromJsonAsync<Post>($"/api/users/{userId}/posts/{postId}");

    public async Task<bool> PostExistsAsync(string userId, string postId)
    {
        var post = await GetPostByIdAsync(userId, postId);
        return post != null;
    }
}
