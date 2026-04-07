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
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Blazing.Mvvm.Analyzers.CodeFixProviders;

/// <summary>
/// Code fix provider for dispose pattern analyzer.
/// Adds IDisposable implementation with proper cleanup code.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DisposePatternCodeFixProvider))]
[Shared]
public sealed class DisposePatternCodeFixProvider : CodeFixProvider
{
    private const string Title = "Implement IDisposable with cleanup";

    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(DiagnosticDescriptors.DisposePatternMissing.Id);

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

        var classDeclaration = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<ClassDeclarationSyntax>()
            .FirstOrDefault();

        if (classDeclaration is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: cancellationToken => ImplementDisposeAsync(
                    context.Document,
                    classDeclaration,
                    cancellationToken),
                equivalenceKey: Title),
            diagnostic);
    }

    /// <summary>
    /// Implements IDisposable interface with Dispose method.
    /// </summary>
    private static async Task<Document> ImplementDisposeAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        var originalText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return document;
        }

        var hasDisposeMethod = classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Any(method => method.Identifier.ValueText == "Dispose" && method.ParameterList.Parameters.Count == 0);

        var alreadyImplementsDisposable = classDeclaration.BaseList?.Types.Any(type => type.Type.ToString() == "IDisposable") == true;
        if (hasDisposeMethod || alreadyImplementsDisposable)
        {
            return document;
        }

        // Add IDisposable to base list
        var disposableType = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.ParseTypeName("IDisposable"));

        var newBaseList = classDeclaration.BaseList is null
            ? SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(disposableType))
            : classDeclaration.BaseList.AddTypes(disposableType);

        newBaseList = newBaseList.NormalizeWhitespace();

        var disposeBody = UsesMessengerRegistration(classDeclaration)
            ? "WeakReferenceMessenger.Default.UnregisterAll(this);\n            GC.SuppressFinalize(this);"
            : "// TODO: Unregister event handlers and dispose resources\n            GC.SuppressFinalize(this);";

        var disposeMethodText = $@"
        public void Dispose()
        {{
            {disposeBody}
        }}";

        var disposeMethod = SyntaxFactory.ParseMemberDeclaration(disposeMethodText) as MethodDeclarationSyntax;
        if (disposeMethod == null)
        {
            return document;
        }

        var newClassDeclaration = classDeclaration
            .WithBaseList(newBaseList)
            .WithOpenBraceToken(
                classDeclaration.OpenBraceToken.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\n"),
                        SyntaxFactory.Whitespace("    "))))
            .WithCloseBraceToken(
                classDeclaration.CloseBraceToken.WithLeadingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.EndOfLine("\n"),
                        SyntaxFactory.Whitespace("    "))))
            .AddMembers(disposeMethod)
            .WithLeadingTrivia(classDeclaration.GetLeadingTrivia());

        // Replace the class declaration in the tree
        var newRoot = compilationUnit.ReplaceNode(classDeclaration, newClassDeclaration);

        // Add using System if not present
        const string usingDirective = "System";
        var hasUsing = compilationUnit.Usings.Any(u =>
            u.Name?.ToString() == usingDirective);

        if (!hasUsing)
        {
            var newUsing = SyntaxFactory.ParseCompilationUnit($"using {usingDirective};\n").Usings[0];

            newRoot = newRoot.AddUsings(newUsing);
        }

        var formattedRoot = Formatter.Format(newRoot, document.Project.Solution.Workspace);

        return document.WithText(NormalizeLineEndings(formattedRoot.ToFullString(), originalText));
    }

    private static SourceText NormalizeLineEndings(string text, SourceText originalText)
    {
        var original = originalText.ToString();
        var hasLeadingBlankLine = original.StartsWith("\r\n", StringComparison.Ordinal) || original.StartsWith("\n", StringComparison.Ordinal);
        var normalized = text.Replace("\r\n", "\n").TrimStart('\r', '\n');

        if (hasLeadingBlankLine)
        {
            normalized = "\n" + normalized;
        }

        return SourceText.From(normalized, originalText.Encoding);
    }

    private static bool UsesMessengerRegistration(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Any(invocation =>
            {
                var identifierText = invocation.Expression.ToString();
                return identifierText.IndexOf("Messenger.Register", StringComparison.Ordinal) >= 0 ||
                       (identifierText.IndexOf("WeakReferenceMessenger", StringComparison.Ordinal) >= 0 &&
                        identifierText.IndexOf("Register", StringComparison.Ordinal) >= 0);
            });
    }
}
