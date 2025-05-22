using System.Reflection;
using Blazing.Mvvm.Components;

namespace Blazing.Mvvm;

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
    /// The parameter resolution mode for the views and view models.
    /// <para>
    /// The default is <see cref="ParameterResolutionMode.None"/>, which disables parameter resolution via the <see cref="IParameterResolver"/> service
    /// and falls back to the default behaviour of the Blazor framework.
    /// </para>
    /// </summary>
    public ParameterResolutionMode ParameterResolutionMode { get; set; } = ParameterResolutionMode.None;

    /// <summary>
    /// Gets the assemblies containing the view models.
    /// </summary>
    internal ICollection<Assembly> ViewModelAssemblies
        => _viewModelAssemblies;

    /// <summary>
    /// Optional base path for the Blazor application.
    /// </summary>
    public string? BasePath { get; set; }

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
