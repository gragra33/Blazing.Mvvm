using Microsoft.AspNetCore.Components;

namespace Blazing.Common;

/// <summary>
/// Represents an element callback for Blazor components.
/// </summary>
public interface IElementCallback
{
    /// <summary>
    /// Gets the event callback to execute.
    /// </summary>
    EventCallback Execute { get; }
}

/// <summary>
/// Represents a generic element callback for Blazor components.
/// </summary>
/// <typeparam name="T">The type of the event callback argument.</typeparam>
public interface IElementCallback<T>
{
    /// <summary>
    /// Gets the event callback to execute with the specified argument type.
    /// </summary>
    EventCallback<T> Execute { get; }
}
