using BEM = Blazing.Common.BEM.CssClass;

namespace Blazing.Lists.Css;

public class CssClass
{
    public static class ListBox
    {
        public const string Root = BEM.LibPrefix + "listbox";
        
        public const string Header = Root + BEM.NameJoin + "header";
        public const string Content = Root + BEM.NameJoin + "content";
        public const string Footer = Root + BEM.NameJoin + "footer";

        public const string Item = Root + BEM.NameJoin + "item";
        
        public static class ItemModifier
        {
            public const string Selected = Item + BEM.ModifierJoin + "selected";
        }
    }
}
