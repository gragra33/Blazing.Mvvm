using System.Reflection;

namespace Blazing.Mvvm.Infrastructure;

/// <summary>
/// Configuration for the library.
/// </summary>
public class LibraryConfiguration
{
    private readonly HashSet<Assembly> _viewModelAssemblies = [];

    /// <summary>
    /// The hosting model of the Blazor application.
    /// </summary>
    public BlazorHostingModelType HostingModelType { get; set; } = BlazorHostingModelType.NotSpecified;

    /// <summary>
    /// Gets the assemblies containing the view models.
    /// </summary>
    internal ICollection<Assembly> ViewModelAssemblies
        => _viewModelAssemblies;

    /// <summary>
    /// Register the view models from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose assembly contains the view models.</typeparam>
    public void RegisterViewModelsFromAssemblyContaining<T>()
    {
        _viewModelAssemblies.Add(typeof(T).Assembly);
    }

    /// <summary>
    /// Register the view models from the assembly containing the specified type.
    /// </summary>
    /// <param name="type">The type whose assembly contains the view models.</param>
    public void RegisterViewModelsFromAssemblyContaining(Type type)
    {
        _viewModelAssemblies.Add(type.Assembly);
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
            _viewModelAssemblies.Add(assembly);
        }
    }
}
