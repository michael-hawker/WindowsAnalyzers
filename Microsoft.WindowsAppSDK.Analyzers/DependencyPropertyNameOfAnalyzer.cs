using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.WindowsAppSDK.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DependencyPropertyNameOfAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DependencyPropertyNameOfAnalyzer"; // TODO: What is this? Is it just the name of the class?

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DependencyPropertyNameOfTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DependencyPropertyNameOfMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.DependencyPropertyNameOfDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Naming"; // TODO: What is a good category for this?

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

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
                    var diagnostic = Diagnostic.Create(Rule, firstArgument.Syntax?.GetLocation(), firstArgument.Value?.ConstantValue);

                    context.ReportDiagnostic(diagnostic);
                }
            }, OperationKind.Invocation);
        });
    }
}
