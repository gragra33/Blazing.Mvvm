using System.Reflection;
using Blazing.Mvvm.Components;

namespace Blazing.Mvvm;

/// <summary>
/// Provides configuration options for the Blazing.Mvvm library, including hosting model, parameter resolution, base path, and view model assembly registration.
/// </summary>
public sealed class LibraryConfiguration
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
    /// Gets or sets a value indicating whether multi-route template support is enabled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When enabled (default), components with multiple <c>@page</c> directives can have all their route templates
    /// cached and used for smart route selection during navigation. The navigation manager will automatically
    /// select the most appropriate route template based on the parameters provided.
    /// </para>
    /// <para>
    /// When disabled, the library reverts to legacy behavior where only the first (simplest) route template
    /// is cached per ViewModel. This option is provided for backward compatibility scenarios.
    /// </para>
    /// <para><b>Default:</b> <c>true</c> (enabled)</para>
    /// <para><b>Recommendation:</b> Keep enabled unless experiencing compatibility issues with existing code.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddMvvm(options =>
    /// {
    ///     options.EnableMultiRouteTemplates = true; // Default behavior
    /// });
    /// </code>
    /// </example>
    public bool EnableMultiRouteTemplates { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a View disposes its transient ViewModel when the View itself is disposed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The dependency injection container keeps every disposable transient service it creates until its scope ends. On Blazor Server
    /// that is the end of the circuit, and on Blazor WebAssembly it is the end of the application, so without this option every visit
    /// to a page leaves its transient ViewModel in memory.
    /// </para>
    /// <para>
    /// When enabled (default), <see cref="MvvmComponentBase{TViewModel}"/> and <see cref="MvvmLayoutComponentBase{TViewModel}"/>
    /// create a ViewModel registered as <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient"/> from its
    /// registration, own it, and dispose it with the View. Its constructor dependencies are still resolved from the View's service
    /// provider, so scoped and singleton services remain shared. Scoped and singleton ViewModels are unaffected, as is
    /// <see cref="MvvmOwningComponentBase{TViewModel}"/>, whose service scope already disposes its ViewModel.
    /// </para>
    /// <para>
    /// When disabled, transient ViewModels are resolved from and owned by the container, as in earlier versions.
    /// </para>
    /// <para><b>Default:</b> <c>true</c> (enabled)</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddMvvm(options =>
    /// {
    ///     options.DisposeTransientViewModels = false; // Restore container-owned transient ViewModels
    /// });
    /// </code>
    /// </example>
    public bool DisposeTransientViewModels { get; set; } = true;

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
