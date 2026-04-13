using System.Text.Json;
using Blazing.Mvvm.Security.Wasm.Models.Auth;
using Microsoft.JSInterop;

namespace Blazing.Mvvm.Security.Wasm.Services;

/// <summary>
/// Provides local storage access for authentication state persistence.
/// Handles serialization and deserialization of user data.
/// </summary>
public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private const string STORAGE_KEY = "authState";

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalStorageService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for interop.</param>
    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Retrieves the stored authentication state from localStorage.
    /// </summary>
    /// <returns>The stored <see cref="CurrentUser"/> if found; otherwise, null.</returns>
    public async Task<CurrentUser?> GetAuthStateAsync()
    {
        try
        {
            var storedUser = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", STORAGE_KEY);

            if (!string.IsNullOrEmpty(storedUser))
            {
                return JsonSerializer.Deserialize<CurrentUser>(storedUser);
            }
        }
        catch (Exception)
        {
            // Swallow exception and return null
        }

        return null;
    }

    /// <summary>
    /// Persists the authentication state to localStorage.
    /// </summary>
    /// <param name="user">The <see cref="CurrentUser"/> to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetAuthStateAsync(CurrentUser user)
    {
        try
        {
            var json = JsonSerializer.Serialize(user);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
        }
        catch (Exception)
        {
            // Swallow exception
        }
    }

    /// <summary>
    /// Removes the stored authentication state from localStorage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task RemoveAuthStateAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", STORAGE_KEY);
        }
        catch (Exception)
        {
            // Swallow exception
        }
    }

    /// <summary>
    /// Determines whether an authentication state is stored in localStorage.
    /// Used to check if RememberMe was enabled during login.
    /// </summary>
    /// <returns>True if authentication state is stored; otherwise, false.</returns>
    public async Task<bool> IsAuthStateStoredAsync()
    {
        try
        {
            var storedUser = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", STORAGE_KEY);
            return !string.IsNullOrEmpty(storedUser);
        }
        catch
        {
            return false;
        }
    }
}
