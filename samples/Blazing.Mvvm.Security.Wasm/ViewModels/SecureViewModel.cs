using System.Security.Claims;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazing.Mvvm.Security.Wasm.ViewModels;

/// <summary>
/// ViewModel for displaying secure user information and claims in a Blazor MVVM page.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class SecureViewModel(AuthenticationStateProvider authProvider) : ViewModelBase
{

    /// <summary>
    /// Gets or sets the list of claims for the authenticated user.
    /// </summary>
    [ObservableProperty]
    private List<Claim>? _claims;


    /// <summary>
    /// Gets or sets a value indicating whether the user is authenticated.
    /// </summary>
    [ObservableProperty]
    private bool _isAuthenticated;


    /// <summary>
    /// Gets or sets the name of the authenticated user.
    /// </summary>
    [ObservableProperty]
    private string? _userName;


    /// <summary>
    /// Gets or sets the authentication type used for the user.
    /// </summary>
    [ObservableProperty]
    private string? _authenticationType;

    /// <summary>
    /// Called when the view model is initialized. Loads authentication state and user claims.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public override async Task OnInitializedAsync()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        IsAuthenticated = user.Identity?.IsAuthenticated ?? false;
        UserName = user.Identity?.Name;
        AuthenticationType = user.Identity?.AuthenticationType;
        Claims = user.Claims.ToList();
    }
}
