using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NsisPlugin.SourceGeneration.Export;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration;

[Generator]
public class ExportSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var nsisActionMethodSyntaxContexts = context.SyntaxProvider.ForAttributeWithMetadataName(Parser.NsisActionAttributeMetadataName, static (node, _) => node is MethodDeclarationSyntax, (syntaxContext, _) => syntaxContext);
        var parseResult = nsisActionMethodSyntaxContexts.Collect().Select(Parse);

        // 此管道不会生成源代码——它仅存在用于报告诊断
        var diagnostics = parseResult.Select(static (result, _) => result.Diagnostics);
        context.RegisterSourceOutput(diagnostics, ReportDiagnostics);

        // 生成源代码
        // ImmutableEquatableArray<TypeGenerationSpec> 实现了序列相等
        // 所以只有在相关代码发生变化时才会生成源代码
        var typeSpecs = parseResult.Select(static (result, _) => result.Types);
        context.RegisterSourceOutput(typeSpecs, EmitSource);

        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:不要使用禁用于分析器的 API")]
        static (ImmutableEquatableArray<TypeGenerationSpec> Types, ImmutableEquatableArray<Diagnostic> Diagnostics) Parse(ImmutableArray<GeneratorAttributeSyntaxContext> methodSyntaxContexts, CancellationToken token)
        {
            // 确保源生成器使用不变文化进行解析。
            // 这可以防止诸如本地化特定的负号（例如，fi-FI 中的 U+2212）等问题
            // 写入生成的源文件中。
            var originalCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                Parser parser = new();
                var typeSpecs = parser.Parse(methodSyntaxContexts, token).ToImmutableEquatableArray();
                var diagnostics = parser.Diagnostics.ToImmutableEquatableArray();
                return (typeSpecs, diagnostics);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }


    private static void ReportDiagnostics(SourceProductionContext context, ImmutableEquatableArray<Diagnostic> diagnostics)
    {
        if (diagnostics.Count == 0) return;
        foreach (var diagnostic in diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void EmitSource(SourceProductionContext context, ImmutableEquatableArray<TypeGenerationSpec> typeSpecs)
    {
        if (typeSpecs.Count == 0) return;

        Emitter emitter = new(context);
        emitter.Emit(typeSpecs);
    }
}
