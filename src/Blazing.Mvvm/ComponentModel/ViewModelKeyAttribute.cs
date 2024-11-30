namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// An attribute used to specify the key for a <c>ViewModel</c> on a <c>View</c>.
/// This attribute should only be used on <c>View</c> components.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ViewModelKeyAttribute : Attribute
{
    /// <summary>
    /// Gets the key associated with the ViewModel.
    /// </summary>
    public object Key { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelKeyAttribute"/> class with the specified key.
    /// </summary>
    /// <param name="key">The key of the <c>ViewModel</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="key"/> is <see langword="null"/>.</exception>
    public ViewModelKeyAttribute(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        Key = key;
    }
}
