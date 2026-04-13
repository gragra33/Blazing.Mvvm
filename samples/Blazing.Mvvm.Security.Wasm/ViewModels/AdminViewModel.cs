using System.Security.Claims;
using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazing.Mvvm.Security.Wasm.ViewModels;

/// <summary>
/// ViewModel for the Admin page. Only accessible by users in the Admin role.
/// </summary>
[ViewModelDefinition(Lifetime = ServiceLifetime.Scoped)]
public sealed partial class AdminViewModel(AuthenticationStateProvider authProvider) : ViewModelBase
{
    /// <summary>
    /// Gets or sets the name of the authenticated admin user.
    /// </summary>
    [ObservableProperty]
    private string _userName = string.Empty;

    /// <summary>
    /// Gets or sets the list of claims for the authenticated user.
    /// </summary>
    [ObservableProperty]
    private List<Claim> _claims = [];

    /// <summary>
    /// Gets or sets a value indicating whether the user is in the Admin role.
    /// </summary>
    [ObservableProperty]
    private bool _isAdmin;

    /// <inheritdoc />
    public override async Task OnInitializedAsync()
    {
        var authState = await authProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        UserName = user.Identity?.Name ?? string.Empty;
        Claims = user.Claims.ToList();
        IsAdmin = user.IsInRole("Admin");

        await base.OnInitializedAsync();
    }
}
