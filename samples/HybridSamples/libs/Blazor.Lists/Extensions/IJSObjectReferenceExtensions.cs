using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazing.Lists;

/// <summary>
/// Provides extension methods for <see cref="IJSObjectReference"/> and <see cref="IJSRuntime"/> to simplify JS interop operations.
/// </summary>
internal static class IJSObjectReferenceExtensions
{
    #region Methods

    /// <summary>
    /// Imports a JavaScript module from the specified file path.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime instance.</param>
    /// <param name="file">The path to the JavaScript file.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the JS object reference.</returns>
    public static async Task<IJSObjectReference> ModuleFactory(this IJSRuntime jsRuntime, string file)
        => await jsRuntime.InvokeAsync<IJSObjectReference>("import", file).ConfigureAwait(false);

    /// <summary>
    /// Scrolls the specified element into view using the provided JS object reference.
    /// </summary>
    /// <param name="jsObjRef">The JS object reference.</param>
    /// <param name="element">The element to scroll into view.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task ScrollIntoViewAsync(this IJSObjectReference? jsObjRef, ElementReference element)
    {
        if (jsObjRef is null) return;
        await jsObjRef.InvokeVoidAsync("scrollIntoView", element).ConfigureAwait(false);
    }

    #endregion
}
