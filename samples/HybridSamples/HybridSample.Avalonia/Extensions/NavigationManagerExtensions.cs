using System.Reflection;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace HybridSample.Avalonia.Extensions;

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
    public static void ForceNavigationManagerUpdate(this IMvvmNavigationManager mvvmNavManager, NavigationManager navManager)
    {
        FieldInfo? prop = mvvmNavManager.GetType().GetField("_navigationManager", BindingFlags.NonPublic | BindingFlags.Instance);
        prop!.SetValue(mvvmNavManager, navManager);
    }
}
