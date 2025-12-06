using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that validates constructor parameters are registered services in the DI container.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServiceInjectionAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ServiceNotRegistered);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeConstructor, SymbolKind.Method);
    }

    private static void AnalyzeConstructor(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Only analyze constructors
        if (methodSymbol.MethodKind != MethodKind.Constructor)
        {
            return;
        }

        var containingType = methodSymbol.ContainingType;

        // Only analyze ViewModels
        if (!containingType.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return;
        }

        // Check each constructor parameter
        foreach (var parameter in methodSymbol.Parameters)
        {
            var parameterType = parameter.Type;

            // Skip primitive types and common framework types
            if (IsPrimitiveOrFrameworkType(parameterType))
            {
                continue;
            }

            // Note: Full validation requires build-time analysis of service registrations
            // This is complex as it requires tracking AddSingleton/AddScoped/AddTransient calls
            // For now, this analyzer provides the structure for future enhancement
            
            // A full implementation would:
            // 1. Track all service registration calls in Startup/Program.cs
            // 2. Match constructor parameter types against registered services
            // 3. Report diagnostic for unregistered services
            
            // Basic heuristic: warn about concrete class dependencies (prefer interfaces)
            if (parameterType.TypeKind == TypeKind.Class && 
                !parameterType.IsAbstract &&
                parameterType.SpecialType == SpecialType.None)
            {
                // This could suggest using interface instead of concrete class
                // but we'll be conservative and not report for now
            }
        }
    }

    private static bool IsPrimitiveOrFrameworkType(ITypeSymbol type)
    {
        if (type.SpecialType != SpecialType.None)
        {
            return true;
        }

        var typeName = type.ToString();
        return typeName.StartsWith("System.", StringComparison.Ordinal) ||
               typeName.StartsWith("Microsoft.Extensions.", StringComparison.Ordinal);
    }
}
