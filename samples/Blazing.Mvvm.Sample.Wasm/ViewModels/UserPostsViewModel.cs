using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Sample.Wasm.Models;
using Blazing.Mvvm.Sample.Wasm.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

#pragma warning disable BL0007 // Component parameter should be auto property - ViewModel pattern requires manual implementation

namespace Blazing.Mvvm.Sample.Wasm.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class UserPostsViewModel : ViewModelBase
{
    private readonly ILogger<UserPostsViewModel> _logger;
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

    public UserPostsViewModel(
        ILogger<UserPostsViewModel> logger,
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
            await LoadUserPostsAsync(UserId);
        }
    }

    private async Task LoadUserPostsAsync(string userId)
    {
        try
        {
            IsLoading = true;
            UserNotFound = false;

            _logger.LogInformation("Loading posts for user {UserId}", userId);

            // Load user
            User = await _usersService.GetUserByIdAsync(userId);

            if (User == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                UserNotFound = true;
                Posts = [];
                return;
            }

            // Load posts
            var posts = await _postsService.GetPostsByUserIdAsync(userId);
            Posts = posts.ToList();

            _logger.LogInformation("Loaded {PostCount} posts for user {UserName}", Posts.Count, User.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading posts for user {UserId}", userId);
            UserNotFound = true;
        }
        finally
        {
            IsLoading = false;
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
    private void BackToUser()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            _logger.LogInformation("Navigating back to user {UserId}", UserId);
            _navigationManager.NavigateTo<UserViewModel>(UserId);
        }
    }

    [RelayCommand]
    private void BackToUsers()
    {
        _logger.LogInformation("Navigating back to users list");
        _navigationManager.NavigateTo<UsersViewModel>();
    }
}
