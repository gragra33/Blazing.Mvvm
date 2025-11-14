using Microsoft.AspNetCore.Components;

namespace Blazing.Common.Extensions;

/// <summary>
/// Provides extension methods for converting markup and types to <see cref="RenderFragment"/> instances in Blazor.
/// </summary>
public static class RenderFragmentExtensions
{
    /// <summary>
    /// Converts a <see cref="MarkupString"/> to a <see cref="RenderFragment"/>.
    /// </summary>
    /// <param name="markup">The markup string to convert.</param>
    /// <returns>A <see cref="RenderFragment"/> representing the markup.</returns>
    public static RenderFragment MarkupStringToRenderFragment(this MarkupString markup)
        => builder => { builder.AddContent(1, markup); };

    /// <summary>
    /// Converts a string containing markup to a <see cref="RenderFragment"/>.
    /// </summary>
    /// <param name="markup">The markup string to convert.</param>
    /// <returns>A <see cref="RenderFragment"/> representing the markup.</returns>
    public static RenderFragment MarkupStringToRenderFragment(this string markup)
        => builder => { builder.AddMarkupContent(1, markup); };

    /// <summary>
    /// Converts a <see cref="Type"/> representing a component to a <see cref="RenderFragment"/>.
    /// </summary>
    /// <param name="contentType">The type of the component to render.</param>
    /// <returns>A <see cref="RenderFragment"/> that renders the specified component type.</returns>
    public static RenderFragment TypeToRenderFragment(this Type contentType)
        => builder =>
        {
            builder.OpenComponent(1, contentType);
            builder.CloseComponent();
        };
}
