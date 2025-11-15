namespace Blazing.Mvvm.Components;

/// <summary>
/// Specifies a key for a <c>ViewModel</c> on a <c>View</c> component, enabling keyed navigation and route mapping in Blazor MVVM applications.
/// This attribute should only be used on <c>View</c> components.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewModelKeyAttribute : Attribute
{
    /// <summary>
    /// Gets the key associated with the ViewModel for keyed navigation.
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelKeyAttribute"/> class with the specified key.
    /// </summary>
    /// <param name="key">The key to associate with the <c>ViewModel</c> for navigation and route mapping.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is <see langword="null"/>.</exception>
    public ViewModelKeyAttribute(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        Key = key;
    }
}
