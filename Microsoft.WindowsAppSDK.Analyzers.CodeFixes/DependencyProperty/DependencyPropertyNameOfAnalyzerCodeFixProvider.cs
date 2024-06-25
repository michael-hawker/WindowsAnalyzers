using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DependencyPropertyNameOfAnalyzerCodeFixProvider)), Shared]
public class DependencyPropertyNameOfAnalyzerCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [DependencyPropertyNameOfId];

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ArgumentSyntax>().First();

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.DependencyPropertyNameOfFixTitle,
                createChangedDocument: cancellationToken => SwitchToNameOfAsync(context.Document, declaration, cancellationToken),
                equivalenceKey: nameof(CodeFixResources.DependencyPropertyNameOfFixTitle)),
            diagnostic);
    }

    private async Task<Document> SwitchToNameOfAsync(Document document, ArgumentSyntax argumentDecl, CancellationToken cancellationToken)
    {
        if (argumentDecl.Expression is LiteralExpressionSyntax literalExpression
            && literalExpression.Kind() == SyntaxKind.StringLiteralExpression)
        {
            var value = literalExpression.Token.ValueText;

            var identifierName = SyntaxFactory.IdentifierName(value);

            var newNameOf = SyntaxFactory.InvocationExpression(
                                SyntaxFactory.IdentifierName("nameof"),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(identifierName))));

            var newArgument = SyntaxFactory.Argument(newNameOf);

            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(argumentDecl, newArgument);

            return document.WithSyntaxRoot(newRoot);
        }

        return document;
    }
}
