using Blazing.Mvvm.Components;

namespace Blazing.Mvvm;

/// <summary>
/// Specifies the mode for resolving parameters.
/// </summary>
public enum ParameterResolutionMode
{
    /// <summary>
    /// This disables parameter resolution via the <see cref="IParameterResolver"/> service.
    /// </summary>
    None,

    /// <summary>
    /// Resolve parameters in the <c>ViewModel</c> only and skips parameter resolution for the <c>View</c>.
    /// </summary>
    ViewModel,

    /// <summary>
    /// Resolve parameters in both the <c>View</c> and the <c>ViewModel</c>.
    /// </summary>
    ViewAndViewModel
}
