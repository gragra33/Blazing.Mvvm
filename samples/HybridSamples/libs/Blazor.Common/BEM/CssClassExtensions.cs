namespace Blazing.Common.BEM;

/// <summary>
/// Provides extension methods for joining BEM (Block Element Modifier) class names and modifiers.
/// </summary>
public static class CssClassExtensions
{
    /// <summary>
    /// Joins the current class name with another name using the BEM naming convention.
    /// </summary>
    /// <param name="this">The base class name.</param>
    /// <param name="name">The name to join.</param>
    /// <returns>The joined class name.</returns>
    public static string JoinName(this string @this, string name)
        => @this + CssClass.NameJoin + name;

    /// <summary>
    /// Joins the current class name with a modifier name using the BEM naming convention.
    /// </summary>
    /// <param name="this">The base class name.</param>
    /// <param name="modifierName">The modifier name to join.</param>
    /// <returns>The joined class name with modifier.</returns>
    public static string JoinModifier(this string @this, string modifierName)
        => @this + CssClass.ModifierJoin + modifierName;
}
