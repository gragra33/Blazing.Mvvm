using Blazing.Mvvm.Components;

namespace Blazing.Mvvm;

/// <summary>
/// Specifies the mode for resolving parameters in Blazing.Mvvm components and view models.
/// </summary>
public enum ParameterResolutionMode
{
    /// <summary>
    /// Disables parameter resolution via the <see cref="IParameterResolver"/> service.
    /// Parameters are not set on the View or ViewModel and default Blazor behavior is used.
    /// </summary>
    None,

    /// <summary>
    /// Resolves parameters in the <c>ViewModel</c> only and skips parameter resolution for the <c>View</c>.
    /// </summary>
    ViewModel,

    /// <summary>
    /// Resolves parameters in both the <c>View</c> and the <c>ViewModel</c>.
    /// </summary>
    ViewAndViewModel
}
