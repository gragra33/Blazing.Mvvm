using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Sample.HybridMaui.Models;
using Blazing.Mvvm.Sample.HybridMaui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

#pragma warning disable BL0007 // Component parameter should be auto property - ViewModel pattern requires manual implementation

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class UserViewModel : ViewModelBase
{
    private readonly ILogger<UserViewModel> _logger;
    private readonly IMvvmNavigationManager _navigationManager;
    private readonly IUsersService _usersService;
    private readonly IPostsService _postsService;

    [ObservableProperty]
    private User? _user;

    [ObservableProperty]
    private List<Post> _posts = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _userNotFound;

    [ViewParameter]
    public string? UserId { get; set; }

    public UserViewModel(
        ILogger<UserViewModel> logger, 
        IMvvmNavigationManager navigationManager,
        IUsersService usersService,
        IPostsService postsService)
    {
        _logger = logger;
        _navigationManager = navigationManager;
        _usersService = usersService;
        _postsService = postsService;
    }

    public override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            await LoadUserDataAsync(UserId);
        }
    }

    private async Task LoadUserDataAsync(string userId)
    {
        try
        {
            IsLoading = true;
            UserNotFound = false;

            _logger.LogInformation("Loading data for user {UserId}", userId);

            // Load user
            User = await _usersService.GetUserByIdAsync(userId);
            
            if (User == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                UserNotFound = true;
                Posts = [];
                return;
            }

            // Load user's posts
            var posts = await _postsService.GetPostsByUserIdAsync(userId);
            Posts = posts.ToList();

            _logger.LogInformation("Loaded user {UserName} with {PostCount} posts", User.Name, Posts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user data for {UserId}", userId);
            UserNotFound = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ViewUserPosts()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            _logger.LogInformation("Navigating to posts for user {UserId}", UserId);
            _navigationManager.NavigateTo<UserPostsViewModel>(UserId);
        }
    }

    [RelayCommand]
    private void ViewPost(string postId)
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            _logger.LogInformation("Navigating to post {PostId} for user {UserId}", postId, UserId);
            _navigationManager.NavigateTo<UserPostViewModel>($"{UserId}/{postId}");
        }
    }

    [RelayCommand]
    private void BackToUsers()
    {
        _logger.LogInformation("Navigating back to users list");
        _navigationManager.NavigateTo<UsersViewModel>();
    }
}
