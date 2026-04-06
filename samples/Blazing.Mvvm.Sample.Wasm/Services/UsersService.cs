using Blazing.Mvvm.Sample.Wasm.Models;

namespace Blazing.Mvvm.Sample.Wasm.Services;

/// <summary>
/// Mock implementation of the users service.
/// </summary>
public class UsersService : IUsersService
{
    private readonly ILogger<UsersService> _logger;
    private static readonly List<User> _users =
    [
        new User { Id = "1", Name = "Alice Johnson", Email = "alice@example.com" },
        new User { Id = "2", Name = "Bob Smith", Email = "bob@example.com" },
        new User { Id = "3", Name = "Charlie Brown", Email = "charlie@example.com" },
        new User { Id = "42", Name = "Deep Thought", Email = "answer@universe.com" },
        new User { Id = "2503", Name = "Test User", Email = "test@example.com" }
    ];

    public UsersService(ILogger<UsersService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        _logger.LogDebug("Getting all users");
        return Task.FromResult<IEnumerable<User>>(_users);
    }

    public Task<User?> GetUserByIdAsync(string userId)
    {
        _logger.LogDebug("Getting user with ID: {UserId}", userId);
        var user = _users.FirstOrDefault(u => u.Id == userId);
        
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found", userId);
        }
        
        return Task.FromResult(user);
    }

    public Task<bool> UserExistsAsync(string userId)
    {
        _logger.LogDebug("Checking if user exists with ID: {UserId}", userId);
        return Task.FromResult(_users.Any(u => u.Id == userId));
    }
}
