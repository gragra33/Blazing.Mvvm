using Blazing.Mvvm.ComponentModel;
using Blazing.Mvvm.Security.Wasm.Models.Auth;
using Blazing.Mvvm.Security.Wasm.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;

namespace Blazing.Mvvm.Security.Wasm.ViewModels.Auth;

/// <summary>
/// ViewModel for handling user login, authentication, and navigation.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Transient)]
public sealed partial class LoginViewModel(
    LocalAuthStateProvider authProvider,
    NavigationManager navigation)
    : ViewModelBase
{

    /// <summary>
    /// Gets or sets the login request containing user credentials.
    /// </summary>
    [ObservableProperty]
    private LoginRequest _loginRequest = new();


    /// <summary>
    /// Gets or sets the error message to display on login failure.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;


    /// <summary>
    /// Gets or sets a value indicating whether a login operation is in progress.
    /// </summary>
    [ObservableProperty]
    private bool _isLoggingIn;


    /// <summary>
    /// Gets or sets the return URL by the MvvmComponentBase from the page property.
    /// </summary>
    [ViewParameter]
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Attempts to log in the user using the provided credentials.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [RelayCommand]
    private async Task SubmitAsync()
    {
        IsLoggingIn = true;
        ErrorMessage = string.Empty;

        try
        {
            await authProvider.LoginAsync(LoginRequest);

            var authState = await authProvider.GetAuthenticationStateAsync();

            if (authState.User.Identity?.IsAuthenticated == true)
            {
                navigation.NavigateTo(ReturnUrl ?? "/");
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    /// <summary>
    /// Clears the login form and error message.
    /// </summary>
    [RelayCommand]
    private void ClearForm()
    {
        LoginRequest = new LoginRequest();
        ErrorMessage = string.Empty;
    }
}
