using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace NsisPlugin.SourceGeneration;

/// <summary>
/// NSIS插件源代码生成器
/// </summary>
[Generator]
public class NsisPluginSourceGenerator : IIncrementalGenerator
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 示例：生成固定的辅助代码
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "HelloGenerated.g.cs",
            SourceText.From("namespace Generated { public static class Hello { public static void Say() => System.Console.WriteLine(\"H2el1lo from Gen!\"); } }", Encoding.UTF8)
        ));
    }
}
