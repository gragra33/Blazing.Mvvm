using Microsoft.AspNetCore.Components;

namespace Blazing.Common;

/*
 * Used with RenderFragment<T> to enable execution of code from
 * within the template outside of the component code eg: close a dialog or toast using custom code
 *
 * to use:
 *
 * A. Component
 * A-1. razor: @TemplateName(new ElementCallback(MethodName) @* display template & point to method *@
 * A-2. code:  private void MethodName()
 *             {
 *                 // execute code within the component eg: close a dialog, refresh action, etc...
 *             }
 * B. Using Component
 * B-1. razor: <component>
 *                 <template>
 *                     <button @onclick="@context.Execute">Close</button>
 *                 </template>
 *             </component>
 */
public class ElementCallback : IElementCallback
{
    public ElementCallback(MulticastDelegate  @delegate)
        => Execute = new EventCallback(null, @delegate);

    public EventCallback Execute { get; }
}
