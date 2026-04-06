using System.Net.Http.Json;
using Blazing.Mvvm.Sample.WebApp.Client.Models;

namespace Blazing.Mvvm.Sample.WebApp.Client.Data;

public class ClientUsersService : IUsersService
{
    private readonly HttpClient _httpClient;

    public ClientUsersService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = await _httpClient.GetFromJsonAsync<IEnumerable<User>>("/api/users");
        return users ?? [];
    }

    public Task<User?> GetUserByIdAsync(string userId)
        => _httpClient.GetFromJsonAsync<User>($"/api/users/{userId}");

    public async Task<bool> UserExistsAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        return user != null;
    }
}
