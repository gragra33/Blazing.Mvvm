using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects a View which implements <c>IDisposable</c> or <c>IAsyncDisposable</c> itself on top of an MVVM
/// component base class. The renderer calls only the View's own method, so the base class disposal, which unsubscribes from
/// the ViewModel, deactivates it and releases a ViewModel the View owns, is bypassed.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ViewDisposalBypassAnalyzer : DiagnosticAnalyzer
{
    private static readonly string[] MvvmBaseMetadataNames =
    [
        AnalyzerConstants.TypeNames.MvvmComponentBaseMetadataName,
        AnalyzerConstants.TypeNames.MvvmOwningComponentBaseMetadataName,
        AnalyzerConstants.TypeNames.MvvmLayoutComponentBaseMetadataName
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.ViewDisposalBypassed);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        // Razor components are compiled from generated code, which is where @implements and @code members land
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            var mvvmBases = MvvmBaseMetadataNames
                .Select(compilationContext.Compilation.GetTypeByMetadataName)
                .Where(type => type is not null)
                .Cast<INamedTypeSymbol>()
                .ToImmutableArray();

            if (mvvmBases.IsEmpty)
            {
                return;
            }

            var disposeMethods = new[]
                {
                    GetInterfaceMethod(compilationContext.Compilation, "System.IDisposable", "Dispose"),
                    GetInterfaceMethod(compilationContext.Compilation, "System.IAsyncDisposable", "DisposeAsync")
                }
                .Where(method => method is not null)
                .Cast<IMethodSymbol>()
                .ToImmutableArray();

            compilationContext.RegisterSymbolAction(symbolContext => AnalyzeNamedType(symbolContext, mvvmBases, disposeMethods), SymbolKind.NamedType);
        });
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, ImmutableArray<INamedTypeSymbol> mvvmBases, ImmutableArray<IMethodSymbol> disposeMethods)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (namedType.TypeKind != TypeKind.Class)
        {
            return;
        }

        var mvvmBase = FindMvvmBase(namedType, mvvmBases);
        if (mvvmBase is null)
        {
            return;
        }

        foreach (var disposeMethod in disposeMethods)
        {
            // Report only on the type that declares the implementation, so classes deriving from it are not reported again
            if (namedType.FindImplementationForInterfaceMember(disposeMethod) is not IMethodSymbol implementation
                || !SymbolEqualityComparer.Default.Equals(implementation.ContainingType, namedType))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.ViewDisposalBypassed,
                implementation.Locations.FirstOrDefault() ?? namedType.Locations.FirstOrDefault() ?? Location.None,
                namedType.Name,
                disposeMethod.ContainingType.Name,
                mvvmBase.Name));
        }
    }

    private static INamedTypeSymbol? FindMvvmBase(INamedTypeSymbol namedType, ImmutableArray<INamedTypeSymbol> mvvmBases)
    {
        for (var current = namedType.BaseType; current is not null; current = current.BaseType)
        {
            foreach (var mvvmBase in mvvmBases)
            {
                if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, mvvmBase))
                {
                    return mvvmBase;
                }
            }
        }

        return null;
    }

    private static IMethodSymbol? GetInterfaceMethod(Compilation compilation, string interfaceMetadataName, string methodName)
        => compilation.GetTypeByMetadataName(interfaceMetadataName)?
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault();
}
