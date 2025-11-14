using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazing.Lists;

internal static class IJSObjectReferenceExtensions
{
    #region Methods

    public static async Task<IJSObjectReference> ModuleFactory(this IJSRuntime jsRuntime, string file)
        => await jsRuntime.InvokeAsync<IJSObjectReference>("import", file).ConfigureAwait(false);

    public static async Task ScrollIntoViewAsync(this IJSObjectReference? jsObjRef, ElementReference element)
    {
        if (jsObjRef is null) return;
        await jsObjRef.InvokeVoidAsync("scrollIntoView", element).ConfigureAwait(false);
    }

    #endregion
}
