using Microsoft.CodeAnalysis;

namespace NsisPlugin.SourceGeneration;

[Generator]
public sealed class InitializerSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var initializerSpecProvider = context.AnalyzerConfigOptionsProvider.Combine(context.CompilationProvider).Select(static (pair, _) => InitializerParser.Parse(pair.Left, pair.Right));
        context.RegisterSourceOutput(initializerSpecProvider, ReportDiagnosticsAndEmitSource);
    }

    private static void ReportDiagnosticsAndEmitSource(SourceProductionContext context, InitializerParseResult spec)
    {
        foreach (var diagnostic in spec.Diagnostics)
        {
            context.ReportDiagnostic(diagnostic.CreateDiagnostic());
        }

        if (!spec.ShouldGenerate) return;
        InitializerEmitter.Emit(context);
    }
}
