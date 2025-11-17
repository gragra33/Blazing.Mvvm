using System.Reflection;

namespace Blazing.Mvvm.Components.Parameter;

/// <summary>
/// Provides a fast delegate-based property setter for a given property on a target object.
/// </summary>
internal sealed class PropertySetter
{
    private readonly Action<object, object> _setter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertySetter"/> class for the specified property.
    /// </summary>
    /// <param name="type">The type that declares the property.</param>
    /// <param name="propertyInfo">The property information for which to create the setter.</param>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a setter.</exception>
    public PropertySetter(Type type, PropertyInfo propertyInfo)
    {
        if (propertyInfo.SetMethod is null)
        {
            throw new InvalidOperationException($"The property '{propertyInfo.Name}' on type '{type.FullName}' does not have a setter.");
        }

        _setter = propertyInfo.SetValue;
    }

    /// <summary>
    /// Sets the value of the property on the specified target object.
    /// </summary>
    /// <param name="target">The object whose property value will be set.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(object target, object value)
        => _setter(target, value);
}
