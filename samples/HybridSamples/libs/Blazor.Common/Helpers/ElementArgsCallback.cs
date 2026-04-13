using Microsoft.AspNetCore.Components;

namespace Blazing.Common;

/// <summary>
/// Provides an implementation of <see cref="IElementArgsCallback"/> that wraps a delegate as an event callback for element arguments.
/// </summary>
public class ElementArgsCallback : IElementArgsCallback
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementArgsCallback"/> class.
    /// </summary>
    /// <param name="delegate">The delegate to be executed as an event callback.</param>
    public ElementArgsCallback(MulticastDelegate  @delegate)
        => Execute = new EventCallback<IElementCallbackArgs>(null, @delegate);

    /// <summary>
    /// Gets the event callback to execute with element callback arguments.
    /// </summary>
    public EventCallback<IElementCallbackArgs> Execute { get; }
}
