namespace Microsoft.AspNetCore.Components;

/// <summary>
/// Provides extension methods for <see cref="ParameterView"/> to help detect parameter changes in Blazor components.
/// </summary>
public static class ParameterViewExtensions
{
    /// <summary>
    /// Determines whether the specified parameter value has changed compared to the value in the <see cref="ParameterView"/>.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    /// <param name="parameters">The <see cref="ParameterView"/> containing the parameters.</param>
    /// <param name="parameterName">The name of the parameter to check.</param>
    /// <param name="parameterValue">The current value of the parameter.</param>
    /// <returns><c>true</c> if the parameter value has changed; otherwise, <c>false</c>.</returns>
    public static bool DidParameterChange<T>(this ParameterView parameters, string parameterName, T? parameterValue)
        => parameters.TryGetValue(parameterName, out T? value) && !EqualityComparer<T>.Default.Equals(value, parameterValue);
}
