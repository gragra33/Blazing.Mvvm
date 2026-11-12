using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects a View which bypasses the disposal of its MVVM component base class. A View that implements
/// <c>IAsyncDisposable</c> itself receives the renderer's only disposal call, so the base class cleanup never runs unless the
/// View calls the base <c>DisposeAsync</c>, which only <c>MvvmOwningComponentBase</c> exposes. A View that re-implements
/// <c>IDisposable</c> is reported only when it does not call the base disposal, because the base class then still releases
/// the ViewModel it owns, but never unsubscribes from or deactivates its ViewModel.
/// </summary>
/// <remarks>
/// A base call counts when it is in the method body or in a local function, and not when it is only inside a lambda or
/// anonymous method, which may never run. A View that re-lists a disposal interface is also checked when the method the
/// interface then maps to is declared on an intermediate class, where it hid the base method without implementing anything.
/// </remarks>
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

            var disposalMethods = new[]
                {
                    GetInterfaceMethod(compilationContext.Compilation, "System.IDisposable", "Dispose"),
                    GetInterfaceMethod(compilationContext.Compilation, "System.IAsyncDisposable", "DisposeAsync")
                }
                .Where(method => method is not null)
                .Cast<IMethodSymbol>()
                .ToImmutableArray();

            if (mvvmBases.IsEmpty || disposalMethods.IsEmpty)
            {
                return;
            }

            compilationContext.RegisterSymbolAction(symbolContext => AnalyzeNamedType(symbolContext, mvvmBases, disposalMethods), SymbolKind.NamedType);
            compilationContext.RegisterOperationBlockAction(blockContext => AnalyzeDisposalBlock(blockContext, mvvmBases, disposalMethods));
        });
    }

    /// <summary>
    /// Reports the cases that have no body of their own to inspect: an abstract <c>DisposeAsync</c> implementation, and an
    /// interface the View re-lists that maps to a method declared on an intermediate class.
    /// </summary>
    private static void AnalyzeNamedType(SymbolAnalysisContext context, ImmutableArray<INamedTypeSymbol> mvvmBases, ImmutableArray<IMethodSymbol> disposalMethods)
    {
        var namedType = (INamedTypeSymbol)context.Symbol;
        if (namedType.TypeKind != TypeKind.Class || FindMvvmBase(namedType, mvvmBases) is not { } mvvmBase)
        {
            return;
        }

        foreach (var interfaceMethod in disposalMethods)
        {
            if (namedType.FindImplementationForInterfaceMember(interfaceMethod) is not IMethodSymbol implementation)
            {
                continue;
            }

            if (SymbolEqualityComparer.Default.Equals(implementation.ContainingType, namedType))
            {
                // A method with a body is checked by AnalyzeDisposalBlock
                if (implementation.IsAbstract && interfaceMethod.Name == "DisposeAsync")
                {
                    Report(context.ReportDiagnostic, implementation.Locations, namedType, interfaceMethod, mvvmBase);
                }

                continue;
            }

            // Skip a method that was already checked as an implementation on the class that declares it, one from metadata,
            // and a mapping inherited from the base class, which is reported once on the class that re-listed the interface
            if (implementation.DeclaringSyntaxReferences.IsEmpty
                || SymbolEqualityComparer.Default.Equals(implementation.ContainingType.FindImplementationForInterfaceMember(interfaceMethod), implementation)
                || SymbolEqualityComparer.Default.Equals(namedType.BaseType?.FindImplementationForInterfaceMember(interfaceMethod), implementation))
            {
                continue;
            }

            if (!ReachesMvvmBaseDisposal(implementation, mvvmBases, context.CancellationToken))
            {
                Report(context.ReportDiagnostic, namedType.Locations, namedType, interfaceMethod, mvvmBase);
            }
        }
    }

    private static void AnalyzeDisposalBlock(OperationBlockAnalysisContext context, ImmutableArray<INamedTypeSymbol> mvvmBases, ImmutableArray<IMethodSymbol> disposalMethods)
    {
        // An implicit implementation shares the interface method's name, and an explicit one lists the interface method
        if (context.OwningSymbol is not IMethodSymbol method
            || (method.Name is not ("Dispose" or "DisposeAsync") && method.ExplicitInterfaceImplementations.IsEmpty)
            || FindMvvmBase(method.ContainingType, mvvmBases) is not { } mvvmBase)
        {
            return;
        }

        foreach (var interfaceMethod in disposalMethods)
        {
            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType.FindImplementationForInterfaceMember(interfaceMethod), method))
            {
                continue;
            }

            var callsBaseDisposal = context.OperationBlocks
                .SelectMany(DescendantsOutsideLambdas)
                .OfType<IInvocationOperation>()
                .Any(invocation => IsBaseDisposalCall(invocation, method, interfaceMethod.Name, mvvmBases, context.CancellationToken));

            if (!callsBaseDisposal)
            {
                Report(context.ReportDiagnostic, method.Locations, method.ContainingType, interfaceMethod, mvvmBase);
            }
        }
    }

    /// <summary>
    /// Enumerates an operation and its descendants, skipping the bodies of lambdas and anonymous methods, which may never run.
    /// </summary>
    private static IEnumerable<IOperation> DescendantsOutsideLambdas(IOperation operation)
    {
        yield return operation;

        foreach (var child in operation.ChildOperations)
        {
            if (child is IAnonymousFunctionOperation)
            {
                continue;
            }

            foreach (var descendant in DescendantsOutsideLambdas(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Determines whether an invocation runs the base disposal: <c>Dispose(bool)</c> on the View itself, or a parameterless
    /// <c>Dispose()</c> or <c>DisposeAsync()</c> on the View that reaches the MVVM base class's disposal (see
    /// <see cref="ReachesMvvmBaseDisposal"/>). Excluded are calls on another object such as a field, the method calling itself,
    /// an unrelated overload, and <c>Dispose(false)</c>, which skips the managed cleanup.
    /// </summary>
    private static bool IsBaseDisposalCall(IInvocationOperation invocation, IMethodSymbol method, string disposalMethodName, ImmutableArray<INamedTypeSymbol> mvvmBases, CancellationToken cancellationToken)
    {
        var target = invocation.TargetMethod;
        if (target.Name != disposalMethodName
            || invocation.Instance is not IInstanceReferenceOperation
            || SymbolEqualityComparer.Default.Equals(target, method))
        {
            return false;
        }

        return target.Parameters.Length switch
        {
            0 => ReachesMvvmBaseDisposal(target, mvvmBases, cancellationToken),
            1 => disposalMethodName == "Dispose"
                && target.Parameters[0].Type.SpecialType == SpecialType.System_Boolean
                && invocation.Arguments[0].Value.ConstantValue is not { HasValue: true, Value: false },
            _ => false
        };
    }

    /// <summary>
    /// Determines whether a parameterless disposal method runs the MVVM base class's cleanup: it is declared on an MVVM base
    /// class, or on an intermediate class whose method calls the base disposal in turn. A method an intermediate class
    /// declared to hide the base method, without calling it, does not. A method compiled into another assembly has no
    /// source to inspect and is assumed to reach the base, so a correct View is never reported for it.
    /// </summary>
    private static bool ReachesMvvmBaseDisposal(IMethodSymbol method, ImmutableArray<INamedTypeSymbol> mvvmBases, CancellationToken cancellationToken)
        => IsMvvmBase(method.ContainingType, mvvmBases)
            || method.DeclaringSyntaxReferences.IsEmpty
            || method.DeclaringSyntaxReferences.Any(reference =>
                SyntaxCallsBaseDisposal(reference.GetSyntax(cancellationToken), method.Name, method.ContainingType, mvvmBases, cancellationToken));

    /// <summary>
    /// A syntactic version of <see cref="IsBaseDisposalCall"/>, for a method declared on another class, whose operations this
    /// analyzer does not see from the type being analyzed: <c>base.Dispose()</c> or <c>base.DisposeAsync()</c> that reaches the
    /// MVVM base class's disposal, or <c>Dispose(bool)</c> with any argument but the literal <see langword="false"/>.
    /// </summary>
    private static bool SyntaxCallsBaseDisposal(SyntaxNode declaration, string disposalMethodName, INamedTypeSymbol declaringType, ImmutableArray<INamedTypeSymbol> mvvmBases, CancellationToken cancellationToken)
        => declaration
            .DescendantNodes(node => node is not AnonymousFunctionExpressionSyntax)
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation =>
            {
                var (receiver, name) = invocation.Expression switch
                {
                    MemberAccessExpressionSyntax memberAccess => (memberAccess.Expression, memberAccess.Name.Identifier.ValueText),
                    IdentifierNameSyntax identifier => ((ExpressionSyntax?)null, identifier.Identifier.ValueText),
                    _ => (null, null)
                };

                if (name != disposalMethodName || receiver is not (null or BaseExpressionSyntax or ThisExpressionSyntax))
                {
                    return false;
                }

                return invocation.ArgumentList.Arguments.Count switch
                {
                    // Without arguments only base.X() leaves the method; X() and this.X() would call the method itself
                    0 => receiver is BaseExpressionSyntax
                        && FindParameterlessMethod(declaringType.BaseType, disposalMethodName) is { } baseMethod
                        && ReachesMvvmBaseDisposal(baseMethod, mvvmBases, cancellationToken),
                    1 => disposalMethodName == "Dispose"
                        && !invocation.ArgumentList.Arguments[0].Expression.IsKind(SyntaxKind.FalseLiteralExpression),
                    _ => false
                };
            });

    /// <summary>
    /// Finds the accessible parameterless instance method that <c>base.Name()</c> binds to from a class whose base is <paramref name="type"/>.
    /// </summary>
    private static IMethodSymbol? FindParameterlessMethod(INamedTypeSymbol? type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var method = current.GetMembers(name)
                .OfType<IMethodSymbol>()
                .FirstOrDefault(candidate => candidate.Parameters.IsEmpty && !candidate.IsStatic && candidate.DeclaredAccessibility != Accessibility.Private);
            if (method is not null)
            {
                return method;
            }
        }

        return null;
    }

    private static bool IsMvvmBase(INamedTypeSymbol type, ImmutableArray<INamedTypeSymbol> mvvmBases)
        => mvvmBases.Any(mvvmBase => SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, mvvmBase));

    private static void Report(Action<Diagnostic> reportDiagnostic, ImmutableArray<Location> locations, INamedTypeSymbol view, IMethodSymbol interfaceMethod, INamedTypeSymbol mvvmBase)
        => reportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.ViewDisposalBypassed,
            locations.FirstOrDefault() ?? view.Locations.FirstOrDefault() ?? Location.None,
            view.Name,
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
