using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazing.Common;

/// <summary>
/// Provides extension methods for <see cref="IJSRuntime"/> and <see cref="IJSObjectReference"/> to simplify JavaScript interop in Blazor.
/// </summary>
public static class IJSObjectReferenceExtensions
{
    #region Methods

    /// <summary>
    /// Gets the path to the common JavaScript file used for Blazor interop.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance.</param>
    /// <returns>The path to the common script file.</returns>
    public static string GetCommonScriptPath (this IJSRuntime jsRuntime)
        => "./_content/Blazor.Common/scripts/common.js";

    /// <summary>
    /// Imports a JavaScript module from the specified file path.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime instance.</param>
    /// <param name="file">The path to the JavaScript file to import.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the JavaScript object reference.</returns>
    public static Task<IJSObjectReference> ModuleFactory(this IJSRuntime jsRuntime, string file)
        => jsRuntime.InvokeAsync<IJSObjectReference>("import", file).AsTask();

    /// <summary>
    /// Gets the height of the specified HTML element using JavaScript interop.
    /// </summary>
    /// <param name="jsObjRef">The JavaScript object reference.</param>
    /// <param name="element">The element reference.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the element height as a string.</returns>
    public static async Task<string> GetElementHeightAsync(this IJSObjectReference? jsObjRef, ElementReference element)
        => await jsObjRef!.InvokeAsync<string>("getElementHeight", element).ConfigureAwait(false);

    /// <summary>
    /// Sets focus to the next element in the DOM using JavaScript interop.
    /// </summary>
    /// <param name="jsObjRef">The JavaScript object reference.</param>
    /// <param name="element">The element reference to start from (optional).</param>
    /// <param name="isReverse">Whether to focus in reverse order.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task FocusNextElement(this IJSObjectReference? jsObjRef, ElementReference? element = null, bool isReverse = false)
        => await jsObjRef!.InvokeVoidAsync("focusNextElement", element, isReverse).ConfigureAwait(false);

    #endregion
}
