using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects a View which bypasses the disposal of its MVVM component base class. A View that implements
/// <c>IAsyncDisposable</c> itself receives the renderer's only disposal call, so the base class cleanup never runs. A View that
/// re-implements <c>IDisposable</c> is reported only when it does not call the base disposal, because the base class then
/// still releases the ViewModel it owns, but never unsubscribes from or deactivates its ViewModel.
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

            if (GetInterfaceMethod(compilationContext.Compilation, "System.IAsyncDisposable", "DisposeAsync") is { } disposeAsyncMethod)
            {
                compilationContext.RegisterSymbolAction(symbolContext => AnalyzeNamedType(symbolContext, mvvmBases, disposeAsyncMethod), SymbolKind.NamedType);
            }

            if (GetInterfaceMethod(compilationContext.Compilation, "System.IDisposable", "Dispose") is { } disposeMethod)
            {
                compilationContext.RegisterOperationBlockAction(blockContext => AnalyzeDisposeBlock(blockContext, mvvmBases, disposeMethod));
            }
        });
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context, ImmutableArray<INamedTypeSymbol> mvvmBases, IMethodSymbol disposeAsyncMethod)
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

        // Report only on the type that declares the implementation, so classes deriving from it are not reported again
        if (namedType.FindImplementationForInterfaceMember(disposeAsyncMethod) is IMethodSymbol implementation
            && SymbolEqualityComparer.Default.Equals(implementation.ContainingType, namedType))
        {
            Report(context.ReportDiagnostic, implementation, disposeAsyncMethod, mvvmBase);
        }
    }

    private static void AnalyzeDisposeBlock(OperationBlockAnalysisContext context, ImmutableArray<INamedTypeSymbol> mvvmBases, IMethodSymbol disposeMethod)
    {
        if (context.OwningSymbol is not IMethodSymbol method
            || FindMvvmBase(method.ContainingType, mvvmBases) is not { } mvvmBase
            || !SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(disposeMethod), method))
        {
            return;
        }

        var callsBaseDisposal = context.OperationBlocks
            .SelectMany(block => block.DescendantsAndSelf())
            .OfType<IInvocationOperation>()
            .Any(invocation => IsBaseDisposalCall(invocation, method));

        if (!callsBaseDisposal)
        {
            Report(context.ReportDiagnostic, method, disposeMethod, mvvmBase);
        }
    }

    /// <summary>
    /// Determines whether an invocation is <c>Dispose()</c> or <c>Dispose(bool)</c> on the View itself, inherited or overridden,
    /// rather than a call on another object such as a field, the method calling itself, an unrelated overload, or
    /// <c>Dispose(false)</c>, which skips the managed cleanup.
    /// </summary>
    private static bool IsBaseDisposalCall(IInvocationOperation invocation, IMethodSymbol method)
    {
        var target = invocation.TargetMethod;
        if (target.Name != "Dispose"
            || invocation.Instance is not IInstanceReferenceOperation
            || SymbolEqualityComparer.Default.Equals(target, method))
        {
            return false;
        }

        return target.Parameters.Length switch
        {
            0 => true,
            1 => target.Parameters[0].Type.SpecialType == SpecialType.System_Boolean
                && invocation.Arguments[0].Value.ConstantValue is not { HasValue: true, Value: false },
            _ => false
        };
    }

    private static void Report(Action<Diagnostic> reportDiagnostic, IMethodSymbol implementation, IMethodSymbol interfaceMethod, INamedTypeSymbol mvvmBase)
        => reportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.ViewDisposalBypassed,
            implementation.Locations.FirstOrDefault() ?? implementation.ContainingType.Locations.FirstOrDefault() ?? Location.None,
            implementation.ContainingType.Name,
            interfaceMethod.ContainingType.Name,
            mvvmBase.Name));

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
