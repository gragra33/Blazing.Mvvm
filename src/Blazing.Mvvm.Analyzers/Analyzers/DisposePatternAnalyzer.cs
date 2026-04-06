using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that ensures ViewModels properly implement IDisposable when using event subscriptions or unmanaged resources.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DisposePatternAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.DisposePatternMissing];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze ViewModels that inherit from Blazing.Mvvm base classes
        if (!InheritsFromViewModelBase(namedTypeSymbol, context.Compilation))
        {
            return;
        }

        // Skip abstract classes and interfaces
        if (namedTypeSymbol.IsAbstract || namedTypeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Skip nested types (helper classes)
        if (namedTypeSymbol.ContainingType != null)
        {
            return;
        }

        // Check if this class or any base class implements IDisposable with a Dispose method
        if (ImplementsIDisposableWithDisposeMethod(namedTypeSymbol))
        {
            // Already implements IDisposable with Dispose method, no warning needed
            return;
        }

        // Check for disposable patterns that require explicit disposal
        bool needsDispose = false;

        // Check for disposable fields/properties that are owned (not injected)
        foreach (var member in namedTypeSymbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                // Skip backing fields for properties (compiler-generated)
                if (field.AssociatedSymbol != null)
                {
                    continue;
                }

                // Skip value types (structs) - they don't need disposal
                if (field.Type.IsValueType)
                {
                    continue;
                }

                // Get the field type
                if (field.Type is not INamedTypeSymbol fieldNamedType)
                {
                    continue;
                }

                // Check if this type implements IDisposable
                if (!ImplementsIDisposable(field.Type))
                {
                    continue;
                }

                // Check if this is a self-disposing Blazing.Mvvm ViewModel
                if (IsSelfDisposingViewModel(fieldNamedType, context.Compilation))
                {
                    continue; // Self-disposing ViewModel - parent doesn't need to dispose it
                }

                // Field implements IDisposable - assume it needs disposal
                // (Even readonly fields may be owned if assigned in constructor)
                needsDispose = true;
                break;
            }
            else if (member is IPropertySymbol property)
            {
                // Skip auto-properties with [ObservableProperty] (managed by source generator)
                if (property.GetAttributes().Any(a => a.AttributeClass?.Name == "ObservablePropertyAttribute"))
                {
                    continue;
                }

                // Skip value types (structs) - they don't need disposal
                if (property.Type.IsValueType)
                {
                    continue;
                }

                // Get the property type
                if (property.Type is not INamedTypeSymbol propType)
                {
                    continue;
                }

                // Check if this type implements IDisposable
                bool propImplementsDisposable = propType.AllInterfaces.Any(i => 
                    i.Name == "IDisposable" && i.ContainingNamespace?.ToString() == "System");

                if (!propImplementsDisposable)
                {
                    continue; // Doesn't implement IDisposable, safe to skip
                }

                // Check if this is a self-disposing Blazing.Mvvm ViewModel
                // A ViewModel is self-disposing if it:
                // 1. Implements IViewModelBase or inherits from ViewModelBase (marker for Blazing.Mvvm ViewModels)
                // 2. AND implements IDisposable with a Dispose() method
                if (IsSelfDisposingViewModel(propType, context.Compilation))
                {
                    continue; // Self-disposing ViewModel - parent doesn't need to dispose it
                }

                // Check if readonly property without initializer (DI-injected)
                bool isReadonly = property.SetMethod == null || 
                                 property.SetMethod.DeclaredAccessibility != Accessibility.Public;
                
                if (isReadonly)
                {
                    // Check for initializer
                    bool hasInitializer = property.DeclaringSyntaxReferences
                        .Select(r => r.GetSyntax(context.CancellationToken))
                        .OfType<PropertyDeclarationSyntax>()
                        .Any(p => p.Initializer != null);
                    
                    if (!hasInitializer)
                    {
                        continue; // DI-injected, not owned
                    }
                }

                // Implements IDisposable but doesn't manage its own disposal
                needsDispose = true;
                break;
            }
        }

        // Only check for event subscriptions if we haven't already found disposable fields
        if (!needsDispose)
        {
            foreach (var syntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is ClassDeclarationSyntax classDeclaration)
                {
                    // Check for event subscriptions (+=)
                    var eventSubscriptions = classDeclaration.DescendantNodes()
                        .OfType<AssignmentExpressionSyntax>()
                        .Where(assignment => assignment.IsKind(SyntaxKind.AddAssignmentExpression))
                        .ToList();

                    // Simple heuristic: if there are event subscriptions, they likely need cleanup
                    if (eventSubscriptions.Any())
                    {
                        needsDispose = true;
                        break;
                    }

                    // Check for explicit Messenger.Register calls (should use RecipientViewModelBase instead)
                    // WeakReferenceMessenger.Send() does NOT require disposal - only .Register() does
                    // IMPORTANT: Only check methods/properties in THIS class, not nested classes
                    var hasMessengerRegistration = classDeclaration.Members
                        .Where(m => m is not ClassDeclarationSyntax) // Exclude nested classes
                        .SelectMany(m => m.DescendantNodes())
                        .OfType<InvocationExpressionSyntax>()
                        .Any(invocation =>
                        {
                            var identifierText = invocation.Expression.ToString();
                            // Check if this is a .Register call (not .Send, .Unregister, etc.)
                            return (identifierText.Contains("Messenger.Register(") ||
                                    identifierText.Contains("Messenger.Register<") ||
                                    identifierText.Contains("WeakReferenceMessenger") && 
                                    (identifierText.Contains(".Register(") || identifierText.Contains(".Register<")));
                        });

                    if (hasMessengerRegistration)
                    {
                        // If using Messenger.Register directly, they should inherit from RecipientViewModelBase
                        needsDispose = true;
                        break;
                    }
                }
            }
        }

        if (needsDispose)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.DisposePatternMissing,
                namedTypeSymbol.Locations[0],
                namedTypeSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ImplementsIDisposable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType)
        {
            return namedType.AllInterfaces.Any(i =>
                i.Name == "IDisposable" && i.ContainingNamespace.ToString() == "System");
        }
        return false;
    }

    /// <summary>
    /// Checks if the type or any of its base classes implements IDisposable with a Dispose() method.
    /// </summary>
    private static bool ImplementsIDisposableWithDisposeMethod(INamedTypeSymbol typeSymbol)
    {
        // Check if this type or any base type has IDisposable in its interface list
        var implementsDisposable = typeSymbol.AllInterfaces.Any(i => 
            i.Name == "IDisposable" && i.ContainingNamespace?.ToString() == "System");
        
        if (!implementsDisposable)
        {
            return false;
        }
        
        // Now check if this type or any base type has a Dispose() method implementation
        var currentType = typeSymbol;
        while (currentType != null)
        {
            var disposeMethod = currentType.GetMembers("Dispose")
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => 
                    m.Parameters.Length == 0 && 
                    m.ReturnsVoid &&
                    !m.IsAbstract);
            
            if (disposeMethod != null)
            {
                return true;
            }
            
            currentType = currentType.BaseType;
        }
        
        return false;
    }

    private static bool InheritsFromViewModelBase(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var viewModelBaseTypes = new[]
        {
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ViewModelBase),
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.RecipientViewModelBase),
            compilation.GetTypeByMetadataName(AnalyzerConstants.TypeNames.ValidatorViewModelBase)
        };

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            if (viewModelBaseTypes.Any(vb => vb != null && SymbolEqualityComparer.Default.Equals(baseType, vb)))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Checks if a type is a self-disposing Blazing.Mvvm ViewModel.
    /// A ViewModel is self-disposing if it implements IViewModelBase or inherits from ViewModelBase
    /// AND also implements IDisposable with a Dispose() method.
    /// </summary>
    private static bool IsSelfDisposingViewModel(INamedTypeSymbol typeSymbol, Compilation _)
    {
        // First check: Does it implement IDisposable with a Dispose() method?
        if (!ImplementsIDisposableWithDisposeMethod(typeSymbol))
        {
            return false;
        }

        // Second check: Does it implement IViewModelBase or inherit from ViewModelBase?
        // Check if implements IViewModelBase interface (directly or inherited)
        var implementsIViewModelBase = typeSymbol.AllInterfaces.Any(i =>
            i.Name == "IViewModelBase" &&
            i.ContainingNamespace?.ToDisplayString() == "Blazing.Mvvm.ComponentModel");

        if (implementsIViewModelBase)
        {
            return true;
        }

        // Also check if any base class implements IViewModelBase
        // This handles cases where nested classes inherit from RecipientViewModelBase
        var currentType = typeSymbol.BaseType;
        while (currentType != null)
        {
            var baseImplementsIViewModelBase = currentType.AllInterfaces.Any(i =>
                i.Name == "IViewModelBase" &&
                i.ContainingNamespace?.ToDisplayString() == "Blazing.Mvvm.ComponentModel");
            
            if (baseImplementsIViewModelBase)
            {
                return true;
            }
            
            currentType = currentType.BaseType;
        }

        // Check if inherits from ViewModelBase, RecipientViewModelBase, or ValidatorViewModelBase
        // by walking the base type chain and checking namespace + name
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            var baseNamespace = baseType.ContainingNamespace?.ToDisplayString();
            var baseName = baseType.Name;
            
            // Check if it's one of the known ViewModel base types
            if (baseNamespace == "Blazing.Mvvm.ComponentModel" &&
                (baseName == "ViewModelBase" || 
                 baseName == "RecipientViewModelBase" || 
                 baseName == "ValidatorViewModelBase"))
            {
                return true;
            }
            
            baseType = baseType.BaseType;
        }

        return false;
    }
}
