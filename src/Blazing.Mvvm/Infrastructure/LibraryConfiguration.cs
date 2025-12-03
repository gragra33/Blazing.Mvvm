using System.Reflection;
using Blazing.Mvvm.Components;

namespace Blazing.Mvvm;

/// <summary>
/// Provides configuration options for the Blazing.Mvvm library, including hosting model, parameter resolution, base path, and view model assembly registration.
/// </summary>
public class LibraryConfiguration
{
    private readonly HashSet<Assembly> _viewModelAssemblies = [];

    /// <summary>
    /// Gets or sets the hosting model of the Blazor application.
    /// </summary>
    public BlazorHostingModelType HostingModelType { get; set; } = BlazorHostingModelType.NotSpecified;

    /// <summary>
    /// Gets or sets the parameter resolution mode for views and view models.
    /// The default is <see cref="ParameterResolutionMode.None"/>, which disables parameter resolution via the <see cref="IParameterResolver"/> service
    /// and falls back to the default behaviour of the Blazor framework.
    /// </summary>
    public ParameterResolutionMode ParameterResolutionMode { get; set; } = ParameterResolutionMode.None;

    /// <summary>
    /// Gets the assemblies containing the view models registered for the application.
    /// </summary>
    internal ICollection<Assembly> ViewModelAssemblies
        => _viewModelAssemblies;

    /// <summary>
    /// Gets or sets the optional base path for the Blazor application, used for subpath hosting scenarios.
    /// </summary>
    [Obsolete("BasePath is no longer required and will be removed in a future version. The base path is now automatically detected from NavigationManager.BaseUri. " +
              "This property is only retained for backward compatibility and explicit override scenarios.", false)]
    public string? BasePath { get; set; }

    /// <summary>
    /// Registers the view models from the assembly containing the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type whose assembly contains the view models.</typeparam>
    public void RegisterViewModelsFromAssemblyContaining<T>()
    {
        _viewModelAssemblies.Add(typeof(T).Assembly);
    }

    /// <summary>
    /// Registers the view models from the assembly containing the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type whose assembly contains the view models.</param>
    public void RegisterViewModelsFromAssemblyContaining(Type type)
    {
        _viewModelAssemblies.Add(type.Assembly);
    }

    /// <summary>
    /// Registers the view models from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies containing the view models.</param>
    public void RegisterViewModelsFromAssembly(params Assembly[] assemblies)
        => RegisterViewModelsFromAssemblies(assemblies);

    /// <summary>
    /// Registers the view models from the specified collection of assemblies.
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
