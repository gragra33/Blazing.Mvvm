using System.Reflection;
using Blazing.Mvvm.Components;
using Microsoft.AspNetCore.Components;

namespace HybridSample.Avalonia.Extensions;

public static class NavigationManagerExtensions
{
    public static void ForceNavigationManagerUpdate(this IMvvmNavigationManager mvvmNavManager, NavigationManager navManager)
    {
        FieldInfo? prop = mvvmNavManager.GetType().GetField("_navigationManager", BindingFlags.NonPublic | BindingFlags.Instance);
        prop!.SetValue(mvvmNavManager, navManager);
    }
}
