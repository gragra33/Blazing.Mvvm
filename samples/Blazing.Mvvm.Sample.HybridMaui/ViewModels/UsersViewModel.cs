using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Components;
using Blazing.Mvvm.Sample.HybridMaui.Models;
using Blazing.Mvvm.Sample.HybridMaui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Blazing.Mvvm.Sample.HybridMaui.ViewModels;

[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class UsersViewModel : ViewModelBase
{
    private readonly ILogger<UsersViewModel> _logger;
    private readonly IMvvmNavigationManager _navigationManager;
    private readonly IUsersService _usersService;

    [ObservableProperty]
    private List<User> _users = [];

    [ObservableProperty]
    private bool _isLoading;

    public UsersViewModel(
        ILogger<UsersViewModel> logger,
        IMvvmNavigationManager navigationManager,
        IUsersService usersService)
    {
        _logger = logger;
        _navigationManager = navigationManager;
        _usersService = usersService;
    }

    public override async Task OnInitializedAsync()
    {
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            IsLoading = true;
            var users = await _usersService.GetAllUsersAsync();
            Users = users.ToList();
            _logger.LogInformation("Loaded {Count} users", Users.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ViewUserDetails(string userId)
    {
        _logger.LogInformation("Navigating to user {UserId}", userId);
        _navigationManager.NavigateTo<UserViewModel>(userId);
    }
}
