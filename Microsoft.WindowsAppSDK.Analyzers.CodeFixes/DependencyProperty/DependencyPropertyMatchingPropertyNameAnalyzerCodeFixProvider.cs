using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DependencyPropertyMatchingPropertyNameAnalyzerCodeFixProvider)), Shared]
public class DependencyPropertyMatchingPropertyNameAnalyzerCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = [DependencyPropertyMatchingPropertyNameId];

    public sealed override FixAllProvider GetFixAllProvider()
    {
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        return WellKnownFixAllProviders.BatchFixer;
    }

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the type declaration identified by the diagnostic.
        var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<VariableDeclaratorSyntax>().FirstOrDefault();

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: CodeFixResources.DependencyPropertyMatchingPropertyNameTitle,
                createChangedDocument: cancellationToken => AddBackingPropertyAsync(context.Document, declaration, cancellationToken),
                equivalenceKey: nameof(CodeFixResources.DependencyPropertyMatchingPropertyNameTitle)),
            diagnostic);
    }

    private async Task<Document> AddBackingPropertyAsync(Document document, VariableDeclaratorSyntax? identifierDecl, CancellationToken cancellationToken)
    {
        if (identifierDecl?.IsKind(SyntaxKind.VariableDeclarator) == true
            && identifierDecl.Identifier.Text.EndsWith("Property")) // Note: Maybe this extra check isn't needed?
        {
            // Get name of property to use without Property suffix of DP Identifier
            var depPropName = identifierDecl.Identifier.Text;
            var newPropName = identifierDecl.Identifier.Text.Substring(0, identifierDecl.Identifier.Text.Length - 8); // TODO: Could we use PolySharp and something like [..^8] instead here?

            // Get containing class to add property to
            if (identifierDecl.Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault() is ClassDeclarationSyntax classDecl
                && identifierDecl.Initializer.Value is InvocationExpressionSyntax invocationSyntax
                && invocationSyntax.ArgumentList.Arguments.Count >= 2
                && invocationSyntax.ArgumentList.Arguments[1].Expression is TypeOfExpressionSyntax propTypeSyntax)
            {
                // Create the inner GetValue/SetValue expressions for our Dependency Property
                var getValueExp = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("GetValue"), // TODO: Not sure if there's a better way to get the actual method here?
                    SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.IdentifierName(depPropName)))));

                var setValueExp = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("SetValue"), // TODO: Not sure if there's a better way to get the actual method here?
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList([
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName(depPropName)), 
                        SyntaxFactory.Argument(SyntaxFactory.IdentifierName("value")) ]))); // TODO: Not sure if there's a better way to get the 'value' keyword here...

                var returnGetStatement = SyntaxFactory.ReturnStatement(
                    SyntaxFactory.CastExpression(propTypeSyntax.Type, getValueExp));

                var newClassDecl = classDecl.AddMembers(
                    SyntaxFactory.PropertyDeclaration(propTypeSyntax.Type, newPropName)
                        .WithModifiers(SyntaxTokenList.Create(SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                        .WithAccessorList(SyntaxFactory.AccessorList(SyntaxList.Create([
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration,
                                SyntaxFactory.Block(returnGetStatement)),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration,
                                SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(setValueExp))) ]))));

                // Return new document with added property to class
                SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                SyntaxNode newRoot = oldRoot.ReplaceNode(classDecl, newClassDecl);
                
                return document.WithSyntaxRoot(newRoot);
            }
        }

        return document;
    }
}
