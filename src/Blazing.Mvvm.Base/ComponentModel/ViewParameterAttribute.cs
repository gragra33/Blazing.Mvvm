namespace Blazing.Mvvm.ComponentModel;

/// <summary>
/// Specifies that a property in a <c>ViewModel</c> should be resolved from parameters passed to the associated <c>View</c> component.
/// Supports optional naming for parameter mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ViewParameterAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewParameterAttribute"/> class.
    /// Marks the property to be resolved from a parameter with the same name as the property.
    /// </summary>
    public ViewParameterAttribute()
    { /* skip */ }

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewParameterAttribute"/> class with a specified parameter name.
    /// </summary>
    /// <param name="name">The name of the parameter to be resolved from.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is <see langword="null"/>, empty, or white-space.</exception>
    public ViewParameterAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Gets the name of the <c>View</c> parameter to be resolved from. If <c>null</c>, the property name is used.
    /// </summary>
    public string? Name { get; }
}
