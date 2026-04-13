using System.Timers;
using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Security.Wasm.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Security.Wasm.ViewModels.Auth;

/// <summary>
/// ViewModel for handling user sign-out, session countdown, and navigation after logout.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public sealed partial class SignOutViewModel(LocalAuthStateProvider authProvider, NavigationManager navigation)
    : ViewModelBase
{
    private System.Timers.Timer? _countdownTimer;
    private int _remainingSeconds = 5;
    private bool _hasLoggedOut; // Prevent multiple logout calls


    /// <summary>
    /// Gets or sets the current session time as a formatted string.
    /// </summary>
    [ObservableProperty]
    private string _sessionTime = DateTime.Now.ToString("hh:mm:ss tt");


    /// <summary>
    /// Gets or sets a value indicating whether the redirect notice is shown.
    /// </summary>
    [ObservableProperty]
    private bool _showRedirectNotice = true;


    /// <summary>
    /// Gets or sets the number of seconds left before redirect.
    /// </summary>
    [ObservableProperty]
    private int _timeLeft = 5;

    /// <summary>
    /// Called when the view model is initialized. Logs out the user and starts the countdown.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnInitializedAsync()
    {
        // Prevent multiple calls to logout (guard against race conditions)
        if (!_hasLoggedOut)
        {
            _hasLoggedOut = true;
            await authProvider.LogoutAsync();
            StartCountdown();
        }
        await base.OnInitializedAsync();
    }

    /// <summary>
    /// Starts the countdown timer for redirect after logout.
    /// </summary>
    private void StartCountdown()
    {
        // Ensure we don't create multiple timers
        if (_countdownTimer != null)
            return;

        _countdownTimer = new System.Timers.Timer(1000);
        _countdownTimer.Elapsed += OnCountdownTick;
        _countdownTimer.AutoReset = true;
        _countdownTimer.Start();
    }

    /// <summary>
    /// Handles the countdown timer tick event, updates time left, and redirects when complete.
    /// </summary>
    /// <param name="sender">The timer object.</param>
    /// <param name="e">The elapsed event arguments.</param>
    private void OnCountdownTick(object? sender, ElapsedEventArgs e)
    {
        _remainingSeconds--;
        TimeLeft = _remainingSeconds;
        NotifyStateChanged();

        if (_remainingSeconds <= 0)
        {
            _countdownTimer?.Stop();
            ShowRedirectNotice = false;
            navigation.NavigateTo("/", false); // Use forceLoad=false to prevent reload
        }
    }

    /// <summary>
    /// Navigates to the login page and stops the countdown timer.
    /// </summary>
    [RelayCommand]
    private void SignInAgain()
    {
        _countdownTimer?.Stop();
        navigation.NavigateTo("/login", false);
    }

    /// <summary>
    /// Navigates to the home page and stops the countdown timer.
    /// </summary>
    [RelayCommand]
    private void GoToHome()
    {
        _countdownTimer?.Stop();
        navigation.NavigateTo("/", false);
    }

    /// <summary>
    /// Disposes the countdown timer and releases resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _countdownTimer?.Stop();
            _countdownTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
