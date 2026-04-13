using BEM = Blazing.Common.BEM.CssClass;

namespace Blazing.Tabs.Css;

public static class CssClass
{
    public const string Root = BEM.LibPrefix + "tabs";
    public const string Header = Root + BEM.NameJoin + "header";
    public const string Button = Root + BEM.NameJoin + "button";
    public const string Title = Root + BEM.NameJoin + "title";
    public const string Panel = Root + BEM.NameJoin + "panel";

    public static class ButtonModifier
    {
        public const string Active = Button + BEM.ModifierJoin + "active";
        public const string Disabled = Button + BEM.ModifierJoin + "disabled";
    }
}
