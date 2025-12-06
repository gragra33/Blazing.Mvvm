using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Blazing.Mvvm.Analyzers.Analyzers;

/// <summary>
/// Analyzer that suggests [NotifyPropertyChangedFor] for dependent computed properties.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NotifyPropertyChangedForAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticDescriptors.NotifyPropertyChangedForMissing);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze ViewModels
        if (!namedTypeSymbol.Name.EndsWith(AnalyzerConstants.Naming.ViewModelSuffix))
        {
            return;
        }

        // Find all computed properties (read-only properties with expression bodies or getters)
        var computedProperties = namedTypeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.IsReadOnly || p.SetMethod == null)
            .ToList();

        if (computedProperties.Count == 0)
        {
            return;
        }

        // Find all observable fields (with ObservableProperty attribute)
        var observableFields = namedTypeSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name == "ObservablePropertyAttribute" ||
                attr.AttributeClass?.Name == "ObservableProperty"))
            .ToList();

        // Find all properties with SetProperty (without [ObservableProperty])
        var propertiesWithSetProperty = namedTypeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod != null && 
                   HasSetPropertyInSetter(p) &&
                   !p.GetAttributes().Any(attr =>
                       attr.AttributeClass?.Name == "ObservablePropertyAttribute" ||
                       attr.AttributeClass?.Name == "ObservableProperty"))
            .ToList();

        // Track which fields/properties we've already reported for each computed property
        var reportedPairs = new HashSet<string>();

        // Analyze each computed property
        foreach (var computedProperty in computedProperties)
        {
            // Analyze the getter to find referenced properties and fields
            foreach (var syntaxReference in computedProperty.DeclaringSyntaxReferences)
            {
                var syntax = syntaxReference.GetSyntax(context.CancellationToken);
                if (syntax is PropertyDeclarationSyntax propertyDeclaration)
                {
                    var referencedIdentifiers = new HashSet<string>();

                    // Check expression body: public string FullName => $"{FirstName} {LastName}";
                    if (propertyDeclaration.ExpressionBody != null)
                    {
                        CollectReferencedIdentifiers(propertyDeclaration.ExpressionBody, referencedIdentifiers);
                    }
                    // Check getter body
                    else if (propertyDeclaration.AccessorList != null)
                    {
                        var getter = propertyDeclaration.AccessorList.Accessors
                            .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                        
                        if (getter != null)
                        {
                            CollectReferencedIdentifiers(getter, referencedIdentifiers);
                        }
                    }

                    // Check if any referenced identifier matches an observable field
                    foreach (var identifier in referencedIdentifiers)
                    {
                        CheckObservableField(observableFields, identifier, computedProperty, context, reportedPairs);
                        CheckPropertyWithSetProperty(propertiesWithSetProperty, identifier, computedProperty, context, reportedPairs);
                    }
                }
            }
        }
    }

    private static void CheckObservableField(
        List<IFieldSymbol> observableFields,
        string identifier,
        IPropertySymbol computedProperty,
        SymbolAnalysisContext context,
        HashSet<string> reportedPairs)
    {
        foreach (var field in observableFields)
        {
            // Check if identifier matches the field name or the generated property name
            var fieldName = field.Name;
            var generatedPropertyName = GetGeneratedPropertyName(fieldName);
            
            if (identifier == fieldName || identifier == generatedPropertyName)
            {
                // Create unique key to prevent duplicate reporting
                var key = $"{field.Name}:{computedProperty.Name}";
                if (reportedPairs.Contains(key))
                {
                    return;
                }

                // Check if the field already has NotifyPropertyChangedFor for this computed property
                var hasNotification = field.GetAttributes().Any(attr =>
                {
                    var attrName = attr.AttributeClass?.Name;
                    var attrFullName = attr.AttributeClass?.ToDisplayString();
                    
                    // Check if this is NotifyPropertyChangedFor attribute
                    bool isNotifyAttribute = attrName == "NotifyPropertyChangedForAttribute" || 
                                           attrName == "NotifyPropertyChangedFor" ||
                                           attrFullName == "CommunityToolkit.Mvvm.ComponentModel.NotifyPropertyChangedForAttribute";
                    
                    if (!isNotifyAttribute)
                        return false;

                    // Check constructor arguments
                    foreach (var arg in attr.ConstructorArguments)
                    {
                        if (arg.Kind == TypedConstantKind.Primitive && arg.Value is string stringValue)
                        {
                            // Match the property name (nameof() is evaluated at compile time)
                            if (stringValue == computedProperty.Name)
                                return true;
                        }
                    }
                    return false;
                });

                if (!hasNotification)
                {
                    reportedPairs.Add(key);
                    
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.NotifyPropertyChangedForMissing,
                        field.Locations[0],
                        field.Name,
                        computedProperty.Name);

                    context.ReportDiagnostic(diagnostic);
                }
                break;
            }
        }
    }

    private static void CheckPropertyWithSetProperty(
        List<IPropertySymbol> properties,
        string identifier,
        IPropertySymbol computedProperty,
        SymbolAnalysisContext context,
        HashSet<string> reportedPairs)
    {
        foreach (var property in properties)
        {
            // Check if identifier matches the property name or backing field name
            if (identifier == property.Name || identifier == GetBackingFieldName(property.Name))
            {
                // For properties with SetProperty, we need to report on the backing field
                // Find the backing field
                var backingField = property.ContainingType.GetMembers()
                    .OfType<IFieldSymbol>()
                    .FirstOrDefault(f => f.Name == GetBackingFieldName(property.Name));

                if (backingField != null)
                {
                    // Create unique key to prevent duplicate reporting
                    var key = $"{backingField.Name}:{computedProperty.Name}";
                    if (reportedPairs.Contains(key))
                    {
                        return;
                    }

                    reportedPairs.Add(key);
                    
                    var diagnostic = Diagnostic.Create(
                        DiagnosticDescriptors.NotifyPropertyChangedForMissing,
                        backingField.Locations[0],
                        backingField.Name,
                        computedProperty.Name);

                    context.ReportDiagnostic(diagnostic);
                }
                break;
            }
        }
    }

    private static bool HasSetPropertyInSetter(IPropertySymbol property)
    {
        if (property.SetMethod == null || property.SetMethod.DeclaringSyntaxReferences.Length == 0)
        {
            return false;
        }

        var syntax = property.SetMethod.DeclaringSyntaxReferences[0].GetSyntax();
        var setterText = syntax?.Parent?.ToString() ?? string.Empty;
        
        return setterText.Contains("SetProperty");
    }

    private static void CollectReferencedIdentifiers(SyntaxNode node, HashSet<string> referencedIdentifiers)
    {
        var identifiers = node.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(id => id.Identifier.ValueText);

        foreach (var identifier in identifiers)
        {
            referencedIdentifiers.Add(identifier);
        }
    }

    private static string GetGeneratedPropertyName(string fieldName)
    {
        // Convert _firstName to FirstName
        if (fieldName.StartsWith("_") && fieldName.Length > 1)
        {
            var propertyName = fieldName.Substring(1);
            return char.ToUpper(propertyName[0]) + propertyName.Substring(1);
        }
        return fieldName;
    }

    private static string GetBackingFieldName(string propertyName)
    {
        // Convert FirstName to _firstName
        if (propertyName.Length > 0)
        {
            return "_" + char.ToLower(propertyName[0]) + propertyName.Substring(1);
        }
        return "_" + propertyName;
    }
}
