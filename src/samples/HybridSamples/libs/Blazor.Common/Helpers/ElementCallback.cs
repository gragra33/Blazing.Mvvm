using Microsoft.AspNetCore.Components;

namespace Blazing.Common;

/// <summary>
/// Used with <c>RenderFragment&lt;T&gt;</c> to enable execution of code from
/// within the template outside of the component code, e.g., close a dialog or toast using custom code.
/// <para>
/// <b>Usage:</b>
/// <list type="bullet">
/// <item>
/// <term>A. Component</term>
/// <description>
/// <c>@TemplateName(new ElementCallback(MethodName))</c> <br/>
/// <c>private void MethodName() { /* execute code within the component eg: close a dialog, refresh action, etc... */ }</c>
/// </description>
/// </item>
/// <item>
/// <term>B. Using Component</term>
/// <description>
/// <c>&lt;component&gt;
///     &lt;template&gt;
///         &lt;button @onclick="@context.Execute"&gt;Close&lt;/button&gt;
///     &lt;/template&gt;
/// &lt;/component&gt;</c>
/// </description>
/// </item>
/// </list>
/// </para>
/// </summary>
public class ElementCallback : IElementCallback
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCallback"/> class.
    /// </summary>
    /// <param name="delegate">The delegate to be executed as an event callback.</param>
    public ElementCallback(MulticastDelegate  @delegate)
        => Execute = new EventCallback(null, @delegate);

    /// <summary>
    /// Gets the event callback to execute.
    /// </summary>
    public EventCallback Execute { get; }
}
