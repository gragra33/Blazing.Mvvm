using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Blazing.Mvvm.Analyzers.CodeFixProviders;

/// <summary>
/// Code fix provider for command pattern analyzer.
/// Adds [RelayCommand] attribute to methods and converts them to proper command pattern.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CommandPatternCodeFixProvider))]
[Shared]
public sealed class CommandPatternCodeFixProvider : CodeFixProvider
{
    private const string _title = "Convert to [RelayCommand]";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticDescriptors.MethodShouldBeCommand.Id];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        var methodDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault();

        if (methodDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: _title,
                createChangedDocument: cancellationToken => ConvertToRelayCommandAsync(
                    context.Document,
                    methodDeclaration,
                    cancellationToken),
                equivalenceKey: _title),
            diagnostic);
    }

    /// <summary>
    /// Converts method to use [RelayCommand] pattern.
    /// Makes method private and adds [RelayCommand] attribute.
    /// </summary>
    private static async Task<Document> ConvertToRelayCommandAsync(
        Document document,
        MethodDeclarationSyntax methodDeclaration,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        // Create [RelayCommand] attribute
        var relayCommandAttribute = SyntaxFactory.Attribute(
            SyntaxFactory.ParseName("RelayCommand"));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(relayCommandAttribute))
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

        // Change method accessibility to private
        var publicModifier = methodDeclaration.Modifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.PublicKeyword));
        var newModifiers = methodDeclaration.Modifiers;
        
        if (publicModifier != default)
        {
            // Replace public with private
            newModifiers = newModifiers.Replace(publicModifier, SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
        }
        else if (!methodDeclaration.Modifiers.Any(m => 
            m.IsKind(SyntaxKind.PrivateKeyword) || 
            m.IsKind(SyntaxKind.ProtectedKeyword) || 
            m.IsKind(SyntaxKind.InternalKeyword)))
        {
            // No accessibility modifier - add private
            newModifiers = newModifiers.Insert(0, SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
        }

        // Create new method with attribute and private modifier
        var newMethodDeclaration = methodDeclaration
            .WithAttributeLists(methodDeclaration.AttributeLists.Add(attributeList))
            .WithModifiers(newModifiers)
            .WithLeadingTrivia(methodDeclaration.GetLeadingTrivia());

        // Replace the method in the tree
        var newRoot = compilationUnit.ReplaceNode(methodDeclaration, newMethodDeclaration);

        // Add using CommunityToolkit.Mvvm.Input if not present
        var usingsToAdd = new[] { "CommunityToolkit.Mvvm.Input" };

        foreach (var usingDirective in usingsToAdd)
        {
            var hasUsing = compilationUnit.Usings.Any(u =>
                u.Name?.ToString() == usingDirective);

            if (!hasUsing)
            {
                var newUsing = SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(usingDirective))
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

                newRoot = newRoot.AddUsings(newUsing);
            }
        }

        return document.WithSyntaxRoot(newRoot);
    }
}
