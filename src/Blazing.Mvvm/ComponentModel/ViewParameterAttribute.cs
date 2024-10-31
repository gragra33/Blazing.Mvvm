namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// An attribute used to mark properties that should be resolved in a <c>ViewModel</c> from parameters passed to the <c>View</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ViewParameterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewParameterAttribute"/> class.
    /// </summary>
    public ViewParameterAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewParameterAttribute"/> class with a specified name.
    /// </summary>
    /// <param name="name">The name of the parameter to be resolved from.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is <see langword="null"/> or white-space.</exception>
    public ViewParameterAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// The name of the <c>View</c> parameter to be resolved from.
    /// </summary>
    public string? Name { get; }
}
