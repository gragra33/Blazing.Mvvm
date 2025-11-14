namespace Blazing.Common.BEM;

public static class CssClassExtensions
{
    public static string JoinName(this string @this, string name)
        => @this + CssClass.NameJoin + name;

    public static string JoinModifier(this string @this, string modifierName)
        => @this + CssClass.ModifierJoin + modifierName;
}
