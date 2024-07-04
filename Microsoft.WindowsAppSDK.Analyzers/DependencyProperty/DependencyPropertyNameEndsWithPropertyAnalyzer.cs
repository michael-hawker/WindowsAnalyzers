using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;
using static Microsoft.WindowsAppSDK.Analyzers.DiagnosticDescriptors;

namespace Microsoft.WindowsAppSDK.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyPropertyNameEndsWithPropertyAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = [DependencyPropertyNameEndsWithProperty];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information

        // We want to look for invocations of DependencyProperty.Register first
        context.RegisterCompilationStartAction(static context =>
        {
            // Get the initial types/symbols we care about inspecting
            if (context.Compilation.GetTypeByMetadataName("Microsoft.UI.Xaml.DependencyProperty") is not { } dependencyPropertyTypeSymbol) return;
            if (dependencyPropertyTypeSymbol.GetMembers("Register").FirstOrDefault() is not IMethodSymbol registerMethodSymbol) return;

            // Run once per executable block of code
            context.RegisterOperationBlockAction(context =>
            {
                foreach(var operation in context.OperationBlocks)
                {
                    if (operation is IFieldInitializerOperation fieldInitializerOperation // Know that we're initializing a field
                        && operation.ChildOperations.FirstOrDefault() is IInvocationOperation invocation
                        && SymbolEqualityComparer.Default.Equals(invocation.TargetMethod, registerMethodSymbol) // Look for our Register method
                        && fieldInitializerOperation.InitializedFields.FirstOrDefault() is IFieldSymbol field
                        && field.Name?.EndsWith("Property") != true) // See if our identifier ends in 'Property'
                    {
                        // For all such symbols, produce a diagnostic.
                        var diagnostic = Diagnostic.Create(DependencyPropertyNameEndsWithProperty, field.Locations.FirstOrDefault(), field.Name);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            });
        });
    }
}
