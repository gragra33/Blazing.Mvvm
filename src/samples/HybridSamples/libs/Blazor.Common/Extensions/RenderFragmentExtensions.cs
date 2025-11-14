using Microsoft.AspNetCore.Components;

namespace Blazing.Common.Extensions;

public static class RenderFragmentExtensions
{
    public static RenderFragment MarkupStringToRenderFragment(this MarkupString markup)
        => builder => { builder.AddContent(1, markup); };

    public static RenderFragment MarkupStringToRenderFragment(this string markup)
        => builder => { builder.AddMarkupContent(1, markup); };

    public static RenderFragment TypeToRenderFragment(this Type contentType)
        => builder =>
        {
            builder.OpenComponent(1, contentType);
            builder.CloseComponent();
        };
}
