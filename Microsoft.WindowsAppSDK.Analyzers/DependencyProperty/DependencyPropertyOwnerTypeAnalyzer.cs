using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyPropertyOwnerTypeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [DependencyPropertyOwnerType];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

        // We want to look for invocations of DependencyProperty.Register first
        context.RegisterCompilationStartAction(static context =>
        {
            // Get the initial types/symbols we care about inspecting
            if (context.Compilation.GetTypeByMetadataName("Microsoft.UI.Xaml.DependencyProperty") is not { } dependencyPropertyTypeSymbol) return;
            if (dependencyPropertyTypeSymbol.GetMembers("Register").FirstOrDefault() is not IMethodSymbol registerMethodSymbol) return;
            var stringType = context.Compilation.GetSpecialType(SpecialType.System_String);

            context.RegisterOperationAction(context =>
            {
                // Look for the DependencyProperty.Register method call
                if (context.Operation is IInvocationOperation operation
                    // && SymbolEqualityComparer.Default.Equals(operation.Type, dependencyPropertyTypeSymbol) // Already covered by the unique register method
                    && SymbolEqualityComparer.Default.Equals(operation.TargetMethod, registerMethodSymbol) // Look for our Register method
                    && operation.Arguments.Length > 3
                    && operation.Arguments.Skip(2).FirstOrDefault() is { } thirdArgument // Grab the third typeof(...) argument
                    && thirdArgument.Value?.Kind == OperationKind.TypeOf
                    && operation.Syntax.Ancestors(true).Where(syntax => syntax.IsKind(SyntaxKind.ClassDeclaration)).FirstOrDefault() is ClassDeclarationSyntax classDeclarationSyntax // Find the containing class identifier
                    && thirdArgument.Syntax.DescendantTokens().Skip(2).FirstOrDefault() is SyntaxToken thirdArgumentTypeName // TODO: Is there a better way to get the token of the typeof operation's parameter
                    && thirdArgumentTypeName.Value?.ToString() != classDeclarationSyntax.Identifier.Value?.ToString()) // Compare
                {
                    // For all such symbols, produce a diagnostic.
                    var diagnostic = Diagnostic.Create(DependencyPropertyOwnerType, thirdArgumentTypeName.GetLocation(), thirdArgumentTypeName.Value?.ToString(), classDeclarationSyntax.Identifier.Value);

                    context.ReportDiagnostic(diagnostic);
                }
            }, OperationKind.Invocation);
        });
    }
}
