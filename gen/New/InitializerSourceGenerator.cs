using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using NsisPlugin.SourceGeneration.Initializer;

namespace NsisPlugin.SourceGeneration;

[Generator]
public sealed class InitializerSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var parseResult = context.AnalyzerConfigOptionsProvider.Combine(context.CompilationProvider).Select(static (pair, _) => Parser.Parse(pair.Left, pair.Right));

        // 报告诊断信息
        var diagnostics = parseResult.Select(static (result, _) => result.Item2);
        context.RegisterSourceOutput(diagnostics, ReportDiagnostics);

        // 生成源码
        var shouldGenerate = parseResult.Select(static (result, _) => result.Item1);
        context.RegisterSourceOutput(shouldGenerate, EmitSource);
    }

    private static void ReportDiagnostics(SourceProductionContext context, ImmutableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.IsDefaultOrEmpty) return;
        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void EmitSource(SourceProductionContext context, bool shouldGenerate)
    {
        if (!shouldGenerate) return;

        Emitter emitter = new(context);
        emitter.Emit();
    }
}
