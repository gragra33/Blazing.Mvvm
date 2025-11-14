namespace Blazing.Lists;

public class ListBoxEventArgs<TItem>
{
    public ListBoxEventArgs(object sender, TItem item)
    {
        Sender = sender;
        Item = item;
    }

    public object Sender { get; }
    public TItem Item { get; }

}
