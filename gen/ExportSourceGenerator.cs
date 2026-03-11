using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NsisPlugin.SourceGeneration.Emitters;
using NsisPlugin.SourceGeneration.Model;
using NsisPlugin.SourceGeneration.Parser;

namespace NsisPlugin.SourceGeneration;

[Generator]
public sealed class ExportSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 解析
        var parseResultProvider = context.SyntaxProvider.ForAttributeWithMetadataName(ExportParser.NsisActionAttributeMetadataName, static (node, _) => node is MethodDeclarationSyntax, ExportParser.ParseMethod);
        var parseResults = parseResultProvider.Collect().Select(ExportParser.ParseMethodResults);

        // 注册生成输出
        context.RegisterSourceOutput(parseResults, ReportDiagnosticsAndEmitSource);
    }

    private static void ReportDiagnosticsAndEmitSource(SourceProductionContext context, ExportParseResult parseResult)
    {
        // 先报告诊断信息
        foreach (var diagnostic in parseResult.Diagnostics) context.ReportDiagnostic(diagnostic.CreateDiagnostic());

        if (parseResult.Types.Count == 0) return;
        ExportEmitter emitter = new(context);
        emitter.Emit(parseResult.Types);
    }
}
