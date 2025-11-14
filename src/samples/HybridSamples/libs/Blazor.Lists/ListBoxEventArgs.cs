namespace Blazing.Lists;

/// <summary>
/// Provides event data for ListBox events, including the sender and the affected item.
/// </summary>
/// <typeparam name="TItem">The type of the item associated with the event.</typeparam>
public class ListBoxEventArgs<TItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ListBoxEventArgs{TItem}"/> class.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="item">The item associated with the event.</param>
    public ListBoxEventArgs(object sender, TItem item)
    {
        Sender = sender;
        Item = item;
    }

    /// <summary>
    /// Gets the source of the event.
    /// </summary>
    public object Sender { get; }

    /// <summary>
    /// Gets the item associated with the event.
    /// </summary>
    public TItem Item { get; }

}
