using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects when MvvmOwningComponentBase should be used instead of MvvmComponentBase
/// for ViewModels that inject scoped services like DbContext.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MvvmOwningComponentBaseUsageAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.MvvmOwningComponentBaseSuggested);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;

        // Check if this is a Blazor component (inherits from ComponentBase)
        if (!InheritsFromComponentBase(namedType))
            return;

        // Check if it inherits from MvvmComponentBase but not MvvmOwningComponentBase
        if (!InheritsFromMvvmComponentBase(namedType) || InheritsFromMvvmOwningComponentBase(namedType))
            return;

        // Get the ViewModel type parameter from MvvmComponentBase<TViewModel>
        var viewModelType = GetViewModelTypeParameter(namedType);
        if (viewModelType == null)
            return;

        // Check if the ViewModel has scoped service dependencies
        if (!HasScopedServiceDependencies(viewModelType, context.Compilation))
            return;

        // Report diagnostic
        var diagnostic = Diagnostic.Create(
            DiagnosticDescriptors.MvvmOwningComponentBaseSuggested,
            namedType.Locations.FirstOrDefault() ?? Location.None,
            namedType.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool InheritsFromComponentBase(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.Name == "ComponentBase" && 
                current.ContainingNamespace.ToString() == "Microsoft.AspNetCore.Components")
                return true;

            current = current.BaseType;
        }
        return false;
    }

    private static bool InheritsFromMvvmComponentBase(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.Name == "MvvmComponentBase" &&
                current.ContainingNamespace.ToString().StartsWith("Blazing.Mvvm"))
                return true;

            current = current.BaseType;
        }
        return false;
    }

    private static bool InheritsFromMvvmOwningComponentBase(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.Name == "MvvmOwningComponentBase" &&
                current.ContainingNamespace.ToString().StartsWith("Blazing.Mvvm"))
                return true;

            current = current.BaseType;
        }
        return false;
    }

    private static INamedTypeSymbol? GetViewModelTypeParameter(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.Name == "MvvmComponentBase" &&
                current.ContainingNamespace.ToString().StartsWith("Blazing.Mvvm") &&
                current.TypeArguments.Length == 1)
            {
                return current.TypeArguments[0] as INamedTypeSymbol;
            }

            current = current.BaseType;
        }
        return null;
    }

    private static bool HasScopedServiceDependencies(INamedTypeSymbol viewModelType, Compilation compilation)
    {
        // Get all constructors
        var constructors = viewModelType.Constructors
            .Where(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public);

        foreach (var constructor in constructors)
        {
            foreach (var parameter in constructor.Parameters)
            {
                if (IsScopedService(parameter.Type, compilation))
                    return true;
            }
        }

        return false;
    }

    private static bool IsScopedService(ITypeSymbol type, Compilation compilation)
    {
        var typeName = type.Name;
        var fullName = type.ToString();

        // Check for common EF Core DbContext patterns
        if (typeName.EndsWith("DbContext") || typeName.EndsWith("Context"))
        {
            // Check if it inherits from DbContext
            var current = type as INamedTypeSymbol;
            while (current != null)
            {
                if (current.Name == "DbContext" &&
                    (current.ContainingNamespace.ToString() == "Microsoft.EntityFrameworkCore" ||
                     current.ContainingNamespace.ToString().StartsWith("Microsoft.EntityFrameworkCore")))
                    return true;

                current = current.BaseType;
            }
        }

        // Check for other common scoped service patterns
        // Repository pattern
        if (typeName.EndsWith("Repository") || fullName.Contains("Repository"))
            return true;

        // Unit of Work pattern
        if (typeName.EndsWith("UnitOfWork") || typeName == "IUnitOfWork")
            return true;

        // Database connection
        if (typeName == "DbConnection" || typeName.EndsWith("Connection"))
            return true;

        return false;
    }
}
