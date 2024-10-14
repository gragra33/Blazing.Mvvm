using System.Reflection;

namespace Blazing.Mvvm.Components.Parameter;

internal sealed class PropertySetter
{
    private readonly Action<object, object> _setter;

    public PropertySetter(Type type, PropertyInfo propertyInfo)
    {
        if (propertyInfo.SetMethod is null)
        {
            throw new InvalidOperationException($"The property '{propertyInfo.Name}' on type '{type.FullName}' does not have a setter.");
        }

        _setter = propertyInfo.SetValue;
    }

    public void SetValue(object target, object value)
        => _setter(target, value);
}
