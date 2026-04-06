using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that detects manual PropertyChanged subscriptions for two-way binding that can be replaced with
/// automatic two-way binding introduced in Blazing.Mvvm v3.2.0.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EventCallbackTwoWayBindingAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
    [
        DiagnosticDescriptors.ManualTwoWayBindingObsolete,
            DiagnosticDescriptors.EventCallbackMissing,
            DiagnosticDescriptors.EventCallbackTypeMismatch
    ];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            // Track components with manual PropertyChanged subscriptions
            var manualSubscriptions = new ConcurrentBag<ManualSubscriptionInfo>();
            
            // Track components with [Parameter] properties
            var componentParameters = new ConcurrentDictionary<INamedTypeSymbol, ComponentParameterInfo>(SymbolEqualityComparer.Default);

            // Collect components and their parameters
            compilationContext.RegisterSymbolAction(symbolContext =>
            {
                var namedType = (INamedTypeSymbol)symbolContext.Symbol;

                if (!IsMvvmComponent(namedType))
                {
                    return;
                }

                var parameterInfo = CollectComponentParameters(namedType);
                if (parameterInfo.Parameters.Count > 0 || parameterInfo.EventCallbacks.Count > 0)
                {
                    componentParameters[namedType] = parameterInfo;
                }
            }, SymbolKind.NamedType);

            // Detect manual PropertyChanged subscriptions
            compilationContext.RegisterSyntaxNodeAction(syntaxContext =>
            {
                var methodDeclaration = (MethodDeclarationSyntax)syntaxContext.Node;

                // Look for OnInitialized method
                if (methodDeclaration.Identifier.Text != "OnInitialized")
                {
                    return;
                }

                var semanticModel = syntaxContext.SemanticModel;
                var containingType = semanticModel.GetDeclaredSymbol(methodDeclaration)?.ContainingType;

                if (containingType == null || !IsMvvmComponent(containingType))
                {
                    return;
                }

                // Look for ViewModel.PropertyChanged += subscriptions
                var subscriptions = methodDeclaration.DescendantNodes()
                    .OfType<AssignmentExpressionSyntax>()
                    .Where(assignment =>
                        assignment.OperatorToken.IsKind(SyntaxKind.PlusEqualsToken) &&
                        assignment.Left.ToString().Contains("PropertyChanged"));

                foreach (var subscription in subscriptions)
                {
                    var handlerName = subscription.Right.ToString();
                    var info = new ManualSubscriptionInfo(
                        containingType,
                        methodDeclaration,
                        subscription,
                        handlerName);
                    
                    manualSubscriptions.Add(info);
                }
            }, SyntaxKind.MethodDeclaration);

            // At compilation end, analyze for obsolete patterns
            compilationContext.RegisterCompilationEndAction(endContext =>
            {
                AnalyzeManualSubscriptions(manualSubscriptions, componentParameters, endContext);
                AnalyzeMissingEventCallbacks(componentParameters, endContext.Compilation, endContext);
            });
        });
    }

    private static void AnalyzeManualSubscriptions(
        IEnumerable<ManualSubscriptionInfo> manualSubscriptions,
        ConcurrentDictionary<INamedTypeSymbol, ComponentParameterInfo> componentParameters,
        CompilationAnalysisContext context)
    {
        foreach (var subscription in manualSubscriptions)
        {
            if (!componentParameters.TryGetValue(subscription.ComponentType, out var paramInfo))
            {
                continue;
            }

            // Get the ViewModel type
            var viewModelType = GetViewModelType(subscription.ComponentType);
            if (viewModelType == null)
            {
                continue;
            }

            // Check if this is a two-way binding pattern
            var handlerMethod = FindHandlerMethod(subscription.ComponentType, subscription.HandlerName);
            if (handlerMethod == null)
            {
                continue;
            }

            // Analyze the handler to see if it invokes EventCallback
            var propertyNames = AnalyzeHandlerForEventCallbackInvocation(handlerMethod);
            
            foreach (var propertyName in propertyNames)
            {
                // Check if component has both Parameter and EventCallback for this property
                var hasParameter = paramInfo.Parameters.ContainsKey(propertyName);
                var hasEventCallback = paramInfo.EventCallbacks.ContainsKey(propertyName);

                // Check if ViewModel has [ViewParameter] for this property
                var hasViewParameter = HasViewParameterProperty(viewModelType, propertyName);

                if (hasParameter && hasEventCallback && hasViewParameter)
                {
                    // This is the obsolete manual two-way binding pattern!
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.ManualTwoWayBindingObsolete,
                        subscription.SubscriptionNode.GetLocation(),
                        propertyName);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static void AnalyzeMissingEventCallbacks(
        ConcurrentDictionary<INamedTypeSymbol, ComponentParameterInfo> componentParameters,
        Compilation compilation,
        CompilationAnalysisContext context)
    {
        foreach (var kvp in componentParameters)
        {
            var componentType = kvp.Key;
            var paramInfo = kvp.Value;

            var viewModelType = GetViewModelType(componentType);
            if (viewModelType == null)
            {
                continue;
            }

            // Check each parameter
            foreach (var parameter in paramInfo.Parameters)
            {
                var propertyName = parameter.Key;
                var propertySymbol = parameter.Value;

                // Check if ViewModel has [ViewParameter] for this property
                if (!HasViewParameterProperty(viewModelType, propertyName))
                {
                    continue;
                }

                // Check if EventCallback is missing
                if (!paramInfo.EventCallbacks.ContainsKey(propertyName))
                {
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.EventCallbackMissing,
                        propertySymbol.Locations[0],
                        propertyName,
                        propertySymbol.Type.ToDisplayString());

                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    // EventCallback exists - check type match
                    var eventCallbackSymbol = paramInfo.EventCallbacks[propertyName];
                    var expectedType = propertySymbol.Type;
                    var actualType = GetEventCallbackTypeArgument(eventCallbackSymbol.Type);

                    if (actualType != null && !SymbolEqualityComparer.Default.Equals(expectedType, actualType))
                    {
                        var diagnostic = Diagnostic.Create(
                            DiagnosticDescriptors.EventCallbackTypeMismatch,
                            eventCallbackSymbol.Locations[0],
                            actualType.ToDisplayString(),
                            expectedType.ToDisplayString(),
                            propertyName);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }

    private static bool IsMvvmComponent(INamedTypeSymbol typeSymbol)
    {
        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            var displayString = baseType.OriginalDefinition.ToDisplayString();
            if (displayString.StartsWith("Blazing.Mvvm.Components.MvvmComponentBase<") ||
                displayString.StartsWith("Blazing.Mvvm.Components.MvvmOwningComponentBase<") ||
                displayString.StartsWith("Blazing.Mvvm.Components.MvvmLayoutComponentBase<"))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    private static INamedTypeSymbol? GetViewModelType(INamedTypeSymbol componentType)
    {
        var baseType = componentType.BaseType;
        while (baseType != null)
        {
            var displayString = baseType.OriginalDefinition.ToDisplayString();
            if ((displayString.StartsWith("Blazing.Mvvm.Components.MvvmComponentBase<") ||
                 displayString.StartsWith("Blazing.Mvvm.Components.MvvmOwningComponentBase<") ||
                 displayString.StartsWith("Blazing.Mvvm.Components.MvvmLayoutComponentBase<")) &&
                baseType.TypeArguments.Length == 1)
            {
                return baseType.TypeArguments[0] as INamedTypeSymbol;
            }
            baseType = baseType.BaseType;
        }
        return null;
    }

    private static ComponentParameterInfo CollectComponentParameters(INamedTypeSymbol typeSymbol)
    {
        var parameters = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);
        var eventCallbacks = new Dictionary<string, IPropertySymbol>(StringComparer.OrdinalIgnoreCase);

        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is not IPropertySymbol property)
            {
                continue;
            }

            var hasParameter = property.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name == "Parameter" || 
                attr.AttributeClass?.Name == "ParameterAttribute");

            if (!hasParameter)
            {
                continue;
            }

            // Check if it's an EventCallback<T>
            if (property.Type is INamedTypeSymbol namedType && namedType.IsGenericType)
            {
                var typeName = namedType.OriginalDefinition.ToDisplayString();
                if (typeName == "Microsoft.AspNetCore.Components.EventCallback<T>" ||
                    typeName.StartsWith("Microsoft.AspNetCore.Components.EventCallback<"))
                {
                    // Extract property name by removing "Changed" suffix
                    if (property.Name.EndsWith("Changed", StringComparison.OrdinalIgnoreCase))
                    {
                        var baseName = property.Name.Substring(0, property.Name.Length - 7); // Remove "Changed"
                        eventCallbacks[baseName] = property;
                    }
                    // If EventCallback doesn't end with "Changed", skip it (not a two-way binding callback)
                    continue;
                }
            }

            // Regular parameter property
            parameters[property.Name] = property;
        }

        return new ComponentParameterInfo(parameters, eventCallbacks);
    }

    private static bool HasViewParameterProperty(INamedTypeSymbol viewModelType, string propertyName)
    {
        foreach (var member in viewModelType.GetMembers())
        {
            if (member is not IPropertySymbol property)
            {
                continue;
            }

            if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var hasViewParameter = property.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name == "ViewParameter" ||
                attr.AttributeClass?.Name == "ViewParameterAttribute");

            if (hasViewParameter)
            {
                return true;
            }
        }

        return false;
    }

    private static IMethodSymbol? FindHandlerMethod(INamedTypeSymbol typeSymbol, string methodName)
    {
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IMethodSymbol method && method.Name == methodName)
            {
                return method;
            }
        }
        return null;
    }

    private static List<string> AnalyzeHandlerForEventCallbackInvocation(IMethodSymbol handlerMethod)
    {
        var propertyNames = new List<string>();

        foreach (var syntaxRef in handlerMethod.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is not MethodDeclarationSyntax methodDeclaration)
            {
                continue;
            }

            // Look for EventCallback InvokeAsync calls
            var invocations = methodDeclaration.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => inv.ToString().Contains("InvokeAsync"));

            foreach (var invocation in invocations)
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                {
                    continue;
                }

                // Extract property name (e.g., "CounterChanged" -> "Counter")
                var callbackName = memberAccess.Expression.ToString();
                if (callbackName.EndsWith("Changed"))
                {
                    var propName = callbackName.Substring(0, callbackName.Length - 7); // Remove "Changed"
                    if (!propertyNames.Contains(propName))
                    {
                        propertyNames.Add(propName);
                    }
                }
            }
        }

        return propertyNames;
    }

    private static ITypeSymbol? GetEventCallbackTypeArgument(ITypeSymbol eventCallbackType)
    {
        if (eventCallbackType is INamedTypeSymbol namedType &&
            namedType.IsGenericType &&
            namedType.TypeArguments.Length == 1)
        {
            return namedType.TypeArguments[0];
        }
        return null;
    }

    private sealed class ManualSubscriptionInfo(
        INamedTypeSymbol componentType,
        MethodDeclarationSyntax methodDeclaration,
        AssignmentExpressionSyntax subscriptionNode,
        string handlerName)
    {
        public INamedTypeSymbol ComponentType { get; } = componentType;
        public MethodDeclarationSyntax MethodDeclaration { get; } = methodDeclaration;
        public AssignmentExpressionSyntax SubscriptionNode { get; } = subscriptionNode;
        public string HandlerName { get; } = handlerName;
    }

    private sealed class ComponentParameterInfo(
        Dictionary<string, IPropertySymbol> parameters,
        Dictionary<string, IPropertySymbol> eventCallbacks)
    {
        public Dictionary<string, IPropertySymbol> Parameters { get; } = parameters;
        public Dictionary<string, IPropertySymbol> EventCallbacks { get; } = eventCallbacks;
    }
}
