using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Blazing.Mvvm.Analyzers.Helpers;

/// <summary>
/// Provides helper methods for analyzers to discover ViewModels and Views across project boundaries.
/// Enables multi-project architecture support where Views and ViewModels are in separate assemblies.
/// </summary>
internal static class CrossProjectAnalyzerHelper
{
    /// <summary>
    /// Gets all ViewModel types from both the current compilation and referenced assemblies.
    /// </summary>
    /// <param name="compilation">The current compilation context.</param>
    /// <returns>A collection of all ViewModel types found.</returns>
    public static ImmutableArray<INamedTypeSymbol> GetAllViewModels(Compilation compilation)
    {
        var viewModels = new List<INamedTypeSymbol>();

        // Get ViewModels from current compilation
        viewModels.AddRange(GetViewModelsFromNamespace(compilation.GlobalNamespace));

        // Get ViewModels from referenced assemblies
        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol != null)
            {
                viewModels.AddRange(GetViewModelsFromAssembly(assemblySymbol));
            }
        }

        return viewModels.ToImmutableArray();
    }

    /// <summary>
    /// Gets all ViewModel types from a specific assembly.
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    /// <returns>A collection of ViewModel types from the assembly.</returns>
    public static IEnumerable<INamedTypeSymbol> GetViewModelsFromAssembly(IAssemblySymbol assembly)
    {
        return GetTypesRecursive(assembly.GlobalNamespace)
            .Where(IsViewModel);
    }

    /// <summary>
    /// Gets all ViewModel types from a namespace and its child namespaces.
    /// </summary>
    /// <param name="namespaceSymbol">The namespace to search.</param>
    /// <returns>A collection of ViewModel types from the namespace.</returns>
    public static IEnumerable<INamedTypeSymbol> GetViewModelsFromNamespace(INamespaceSymbol namespaceSymbol)
    {
        return GetTypesRecursive(namespaceSymbol)
            .Where(IsViewModel);
    }

    /// <summary>
    /// Determines if a type is a ViewModel based on naming convention and inheritance.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a ViewModel; otherwise, false.</returns>
    public static bool IsViewModel(INamedTypeSymbol type)
    {
        // Must be a concrete class
        if (type.TypeKind != TypeKind.Class || type.IsAbstract)
            return false;

        // Check if it ends with "ViewModel"
        if (type.Name.EndsWith("ViewModel", StringComparison.Ordinal))
            return true;

        // Check if it inherits from Blazing.Mvvm ViewModelBase types
        return InheritsFromViewModelBase(type);
    }

    /// <summary>
    /// Checks if a type inherits from any Blazing.Mvvm ViewModel base class.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type inherits from a Blazing.Mvvm ViewModel base class; otherwise, false.</returns>
    /// <remarks>
    /// Checks for inheritance from all Blazing.Mvvm ViewModel base classes:
    /// <list type="bullet">
    /// <item><description>ViewModelBase</description></item>
    /// <item><description>RecipientViewModelBase</description></item>
    /// <item><description>RecipientViewModelBase&lt;TMessage&gt;</description></item>
    /// <item><description>ValidatorViewModelBase</description></item>
    /// </list>
    /// All must be in the Blazing.Mvvm.ComponentModel namespace to avoid false matches
    /// with custom ViewModelBase classes in other namespaces.
    /// </remarks>
    public static bool InheritsFromViewModelBase(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            var currentNamespace = current.ContainingNamespace?.ToDisplayString();
            
            // Check if it's in the Blazing.Mvvm.ComponentModel namespace
            if (currentNamespace == "Blazing.Mvvm.ComponentModel")
            {
                // Check for all Blazing.Mvvm ViewModel base class names
                // RecipientViewModelBase can be generic (RecipientViewModelBase<TMessage>) or non-generic
                if (current.Name == "ViewModelBase" ||
                    current.Name == "RecipientViewModelBase" ||
                    current.Name == "ValidatorViewModelBase")
                {
                    return true;
                }
            }
            
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Gets all [ViewParameter] properties from a ViewModel type, including inherited properties.
    /// </summary>
    /// <param name="viewModelType">The ViewModel type to analyze.</param>
    /// <returns>A dictionary mapping property names to their types.</returns>
    public static ImmutableDictionary<string, ITypeSymbol> GetViewParameters(INamedTypeSymbol viewModelType)
    {
        var parameters = new Dictionary<string, ITypeSymbol>(StringComparer.OrdinalIgnoreCase);

        // Walk up the inheritance chain
        var current = viewModelType;
        while (current != null && current.SpecialType != SpecialType.System_Object)
        {
            foreach (var member in current.GetMembers())
            {
                if (member is not IPropertySymbol property)
                    continue;

                // Check for [ViewParameter] attribute
                foreach (var attribute in property.GetAttributes())
                {
                    var attrName = attribute.AttributeClass?.Name;
                    if (attrName == "ViewParameter" || attrName == "ViewParameterAttribute")
                    {
                        // Use the property name or custom name from attribute
                        string parameterName = property.Name;
                        
                        // Check for custom name in attribute constructor
                        if (attribute.ConstructorArguments.Length > 0 &&
                            attribute.ConstructorArguments[0].Value is string customName &&
                            !string.IsNullOrWhiteSpace(customName))
                        {
                            parameterName = customName;
                        }
                        
                        // Add if not already present (derived class properties take precedence)
                        if (!parameters.ContainsKey(parameterName))
                        {
                            parameters[parameterName] = property.Type;
                        }
                        break;
                    }
                }
            }
            
            current = current.BaseType;
        }

        return parameters.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Finds a ViewModel type by name in the compilation (current and referenced assemblies).
    /// </summary>
    /// <param name="compilation">The compilation context.</param>
    /// <param name="viewModelName">The name of the ViewModel to find.</param>
    /// <returns>The ViewModel type if found; otherwise, null.</returns>
    public static INamedTypeSymbol? FindViewModel(Compilation compilation, string viewModelName)
    {
        // Try current compilation first
        var types = compilation.GetSymbolsWithName(viewModelName, SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .Where(IsViewModel)
            .ToList();

        if (types.Count > 0)
            return types.First();

        // Search in referenced assemblies
        foreach (var reference in compilation.References)
        {
            var assemblySymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
            if (assemblySymbol != null)
            {
                var viewModel = GetViewModelsFromAssembly(assemblySymbol)
                    .FirstOrDefault(vm => vm.Name == viewModelName);
                
                if (viewModel != null)
                    return viewModel;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the ViewModel type parameter from a View component type.
    /// </summary>
    /// <param name="componentType">The component type (View) to analyze.</param>
    /// <returns>The ViewModel type if found; otherwise, null.</returns>
    public static INamedTypeSymbol? GetViewModelFromComponent(INamedTypeSymbol componentType)
    {
        var current = componentType.BaseType;
        while (current != null)
        {
            // Look for MvvmComponentBase<TViewModel> or MvvmOwningComponentBase<TViewModel>
            if ((current.Name == "MvvmComponentBase" || 
                 current.Name == "MvvmOwningComponentBase" ||
                 current.Name == "MvvmLayoutComponentBase") &&
                current.ContainingNamespace.ToString().StartsWith("Blazing.Mvvm") &&
                current.TypeArguments.Length == 1)
            {
                return current.TypeArguments[0] as INamedTypeSymbol;
            }

            current = current.BaseType;
        }
        return null;
    }

    /// <summary>
    /// Recursively gets all types from a namespace and its child namespaces.
    /// </summary>
    /// <param name="namespaceSymbol">The namespace to traverse.</param>
    /// <returns>All types found in the namespace hierarchy.</returns>
    private static IEnumerable<INamedTypeSymbol> GetTypesRecursive(INamespaceSymbol namespaceSymbol)
    {
        // Get types in current namespace
        foreach (var type in namespaceSymbol.GetTypeMembers())
        {
            yield return type;
            
            // Also get nested types
            foreach (var nestedType in GetNestedTypes(type))
            {
                yield return nestedType;
            }
        }

        // Recursively get types from child namespaces
        foreach (var childNamespace in namespaceSymbol.GetNamespaceMembers())
        {
            foreach (var type in GetTypesRecursive(childNamespace))
            {
                yield return type;
            }
        }
    }

    /// <summary>
    /// Gets all nested types from a type, recursively.
    /// </summary>
    /// <param name="type">The type to get nested types from.</param>
    /// <returns>All nested types found.</returns>
    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nestedType in type.GetTypeMembers())
        {
            yield return nestedType;
            
            // Recursively get nested types
            foreach (var deeplyNestedType in GetNestedTypes(nestedType))
            {
                yield return deeplyNestedType;
            }
        }
    }
}
