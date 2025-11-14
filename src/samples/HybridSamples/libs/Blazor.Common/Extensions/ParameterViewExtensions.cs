namespace Microsoft.AspNetCore.Components;

public static class ParameterViewExtensions
{
    public static bool DidParameterChange<T>(this ParameterView parameters, string parameterName, T? parameterValue)
        => parameters.TryGetValue(parameterName, out T? value) && !EqualityComparer<T>.Default.Equals(value, parameterValue);
}
