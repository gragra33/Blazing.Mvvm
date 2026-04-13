using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace HybridSample.MAUI.Extensions;

/// <summary>
/// Extension methods for updating the navigation manager in MVVM scenarios.
/// </summary>
public static class NavigationManagerExtensions
{
    /// <summary>
    /// Forces the update of the internal <see cref="NavigationManager"/> instance in an <see cref="IMvvmNavigationManager"/>.
    /// </summary>
    /// <param name="mvvmNavManager">The MVVM navigation manager to update.</param>
    /// <param name="navManager">The navigation manager to set.</param>
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2075:UnrecognizedReflectionPattern",
        Justification = "The private _navigationManager field is guaranteed to exist in MvvmNavigationManager implementation.")]
    public static void ForceNavigationManagerUpdate(this IMvvmNavigationManager mvvmNavManager, NavigationManager navManager)
    {
        FieldInfo? prop = mvvmNavManager.GetType().GetField("_navigationManager", BindingFlags.NonPublic | BindingFlags.Instance);
        prop!.SetValue(mvvmNavManager, navManager);
    }
}
