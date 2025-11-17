namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Provides extension methods for <see cref="ElementReference"/> to check for null or default values in Blazor components.
/// </summary>
public static class ElementReferenceExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="ElementReference"/> is null or default.
    /// </summary>
    /// <param name="reference">The element reference to check.</param>
    /// <returns><c>true</c> if the reference is null or default; otherwise, <c>false</c>.</returns>
    public static bool IsNull(this ElementReference reference)
        => EqualityComparer<ElementReference>.Default.Equals(reference, default);

    /// <summary>
    /// Determines whether the specified <see cref="ElementReference"/> is not null or default.
    /// </summary>
    /// <param name="reference">The element reference to check.</param>
    /// <returns><c>true</c> if the reference is not null or default; otherwise, <c>false</c>.</returns>
    public static bool IsNotNull(this ElementReference reference)
        => !reference.IsNull();
}
