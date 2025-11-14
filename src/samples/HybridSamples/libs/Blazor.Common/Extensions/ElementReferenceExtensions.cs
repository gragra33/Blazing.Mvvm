namespace Microsoft.AspNetCore.Components;

public static class ElementReferenceExtensions
{
    public static bool IsNull(this ElementReference reference)
        => EqualityComparer<ElementReference>.Default.Equals(reference, default);

    public static bool IsNotNull(this ElementReference reference)
        => !reference.IsNull();
}
