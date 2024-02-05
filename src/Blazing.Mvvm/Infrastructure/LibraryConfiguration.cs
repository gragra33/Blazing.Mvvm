using System.Reflection;

namespace Blazing.Mvvm.Infrastructure;

/// <summary>
/// Configuration for the library.
/// </summary>
public class LibraryConfiguration
{
    /// <summary>
    /// The hosting model of the Blazor application.
    /// </summary>
    public BlazorHostingModelType HostingModelType { get; set; } = BlazorHostingModelType.NotSpecified;

    /// <summary>
    /// A flag to enable the navigation manager
    /// </summary>
    public bool EnableNavigationManager { get; set; } = true;

    internal readonly HashSet<Assembly> ViewModelAssemblies = [];

    /// <summary>
    /// Register the view models from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly contains the view models.</typeparam>
    public void RegisterViewModelsFromAssemblyContaining<T>()
    {
        ViewModelAssemblies.Add(typeof(T).Assembly);
    }

    /// <summary>
    /// Register the view models from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies containing the view models.</param>
    public void RegisterViewModelsFromAssembly(params Assembly[] assemblies)
        => RegisterViewModelsFromAssembly(assemblies);

    /// <summary>
    /// Register the view models from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies containing the view models.</param>
    public void RegisterViewModelsFromAssembly(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
        {
            ViewModelAssemblies.Add(assembly);
        }
    }
}
