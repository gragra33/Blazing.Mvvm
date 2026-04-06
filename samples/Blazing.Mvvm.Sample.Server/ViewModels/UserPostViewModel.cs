using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Sample.Server.Models;
using Blazing.Mvvm.Sample.Server.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

#pragma warning disable BL0007 // Component parameter should be auto property - ViewModel pattern requires manual implementation

namespace Blazing.Mvvm.Sample.Server.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class UserPostViewModel : ViewModelBase
{
    private readonly ILogger<UserPostViewModel> _logger;
    private readonly IMvvmNavigationManager _navigationManager;
    private readonly IUsersService _usersService;
    private readonly IPostsService _postsService;

    [ObservableProperty]
    private User? _user;

    [ObservableProperty]
    private Post? _post;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _postNotFound;

    [ViewParameter]
    public string? UserId { get; set; }

    [ViewParameter]
    public string? PostId { get; set; }

    public UserPostViewModel(
        ILogger<UserPostViewModel> logger, 
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
        if (!string.IsNullOrEmpty(UserId) && !string.IsNullOrEmpty(PostId))
        {
            await LoadPostDataAsync(UserId, PostId);
        }
    }

    private async Task LoadPostDataAsync(string userId, string postId)
    {
        try
        {
            IsLoading = true;
            PostNotFound = false;

            _logger.LogInformation("Loading post {PostId} for user {UserId}", postId, userId);

            // Load user
            User = await _usersService.GetUserByIdAsync(userId);
            
            if (User == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                PostNotFound = true;
                return;
            }

            // Load post
            Post = await _postsService.GetPostByIdAsync(userId, postId);
            
            if (Post == null)
            {
                _logger.LogWarning("Post {PostId} not found for user {UserId}", postId, userId);
                PostNotFound = true;
                return;
            }

            _logger.LogInformation("Loaded post '{PostTitle}' for user {UserName}", Post.Title, User.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading post {PostId} for user {UserId}", postId, userId);
            PostNotFound = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BackToPosts()
    {
        if (!string.IsNullOrEmpty(UserId))
        {
            _logger.LogInformation("Navigating back to posts for user {UserId}", UserId);
            _navigationManager.NavigateTo<UserPostsViewModel>(UserId);
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
