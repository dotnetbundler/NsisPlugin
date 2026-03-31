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
        // 解析使用 [NsisAction] 的方法
        var nsisActionMethodSyntaxContexts = context.SyntaxProvider.ForAttributeWithMetadataName(Parser.NsisActionAttributeMetadataName, static (node, _) => node is MethodDeclarationSyntax, (syntaxContext, _) => syntaxContext);
        var parseResult = nsisActionMethodSyntaxContexts.Collect().Select(MethodParse);

        // 解析是否使用共享入口初始化
        var useSharedEntryInit = context.AnalyzerConfigOptionsProvider.Select(Parser.UseSharedEntryInit);

        // 此管道不会生成源代码——它仅存在用于报告诊断
        var diagnostics = parseResult.Select(static (result, _) => result.Diagnostics);
        context.RegisterSourceOutput(diagnostics, ReportDiagnostics);

        // 生成源代码
        // ImmutableEquatableArray<TypeGenerationSpec> 实现了序列相等（优化增量生成器的性能），
        // 因此只有在类型规范列表发生变化时才会触发源代码生成。
        var emission = parseResult.Combine(useSharedEntryInit).Select(static (pair, _) => (pair.Left.Types, pair.Right));
        context.RegisterSourceOutput(emission, EmitSource);

        // 解析方法
        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:不要使用禁用于分析器的 API")]
        static (ImmutableEquatableArray<TypeGenerationSpec> Types, ImmutableEquatableArray<Diagnostic> Diagnostics) MethodParse(ImmutableArray<GeneratorAttributeSyntaxContext> methodSyntaxContexts, CancellationToken token)
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

    private static void EmitSource(SourceProductionContext context, (ImmutableEquatableArray<TypeGenerationSpec> Types, bool UseSharedEntryInit) emission)
    {
        if (emission.Types.Count == 0) return;

        if (emission.UseSharedEntryInit)
        {
            SharedEntryInitEmitter emitter2 = new(context);
            emitter2.Emit(emission.Types);
            return;
        }

        IndependentEmitter emitter = new(context);
        emitter.Emit(emission.Types);
    }
}
