using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazing.Common;

public static class IJSObjectReferenceExtensions
{
    #region Methods

    public static string GetCommonScriptPath (this IJSRuntime jsRuntime)
        => "./_content/Blazor.Common/scripts/common.js";

    public static Task<IJSObjectReference> ModuleFactory(this IJSRuntime jsRuntime, string file)
        => jsRuntime.InvokeAsync<IJSObjectReference>("import", file).AsTask();

    public static async Task<string> GetElementHeightAsync(this IJSObjectReference? jsObjRef, ElementReference element)
        => await jsObjRef!.InvokeAsync<string>("getElementHeight", element).ConfigureAwait(false);

    public static async Task FocusNextElement(this IJSObjectReference? jsObjRef, ElementReference? element = null, bool isReverse = false)
        => await jsObjRef.InvokeVoidAsync("focusNextElement", element, isReverse).ConfigureAwait(false);

    #endregion
}
