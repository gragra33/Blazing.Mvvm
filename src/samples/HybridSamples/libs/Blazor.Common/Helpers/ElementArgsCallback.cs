using Microsoft.AspNetCore.Components;

namespace Blazing.Common;

public class ElementArgsCallback : IElementArgsCallback
{
    public ElementArgsCallback(MulticastDelegate  @delegate)
        => Execute = new EventCallback<IElementCallbackArgs>(null, @delegate);

    public EventCallback<IElementCallbackArgs> Execute { get; }
}
