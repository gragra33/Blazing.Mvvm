using System.Reflection;

namespace Blazing.Mvvm.Infrastructure;

/// <summary>
/// Configuration for the library.
/// </summary>
public class LibraryConfiguration
{
    private readonly HashSet<Assembly> viewModelAssemblies = [];

    /// <summary>
    /// The hosting model of the Blazor application.
    /// </summary>
    public BlazorHostingModelType HostingModelType { get; set; } = BlazorHostingModelType.NotSpecified;

    /// <summary>
    /// A flag to enable the Mvvm navigation manager
    /// </summary>
    public bool EnableMvvmNavigationManager { get; set; } = true;

    /// <summary>
    /// Register the view models from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly contains the view models.</typeparam>
    public void RegisterViewModelsFromAssemblyContaining<T>()
    {
        viewModelAssemblies.Add(typeof(T).Assembly);
    }

    /// <summary>
    /// Register the view models from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies containing the view models.</param>
    public void RegisterViewModelsFromAssembly(params Assembly[] assemblies)
        => RegisterViewModelsFromAssemblies(assemblies);

    /// <summary>
    /// Register the view models from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies containing the view models.</param>
    public void RegisterViewModelsFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            viewModelAssemblies.Add(assembly);
        }
    }

    /// <summary>
    /// Gets the assemblies to scan for view models.
    /// If no assemblies are specified, the current domain assemblies are returned.
    /// </summary>
    /// <returns>The assemblies to scan.</returns>
    internal IEnumerable<Assembly> GetScanAssemblies()
    {
        return viewModelAssemblies.Count > 0
            ? viewModelAssemblies
            : AppDomain.CurrentDomain.GetAssemblies();
    }
}
