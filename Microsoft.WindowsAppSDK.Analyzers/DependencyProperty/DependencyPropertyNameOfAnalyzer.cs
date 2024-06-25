using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyPropertyNameOfAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [DependencyPropertyNameOf];

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
                    && operation.Arguments.FirstOrDefault() is { } firstArgument
                    && firstArgument.Value?.Kind == OperationKind.Literal // Check if it's a literal string
                    && SymbolEqualityComparer.Default.Equals(firstArgument.Value?.Type, stringType))
                {
                    // For all such symbols, produce a diagnostic.
                    var diagnostic = Diagnostic.Create(DependencyPropertyNameOf, firstArgument.Syntax?.GetLocation(), firstArgument.Value?.ConstantValue);

                    context.ReportDiagnostic(diagnostic);
                }
            }, OperationKind.Invocation);
        });
    }
}
