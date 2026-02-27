using Microsoft.CodeAnalysis;

namespace NsisPlugin.SourceGeneration;

/// <summary>
/// NSIS插件导出源代码生成器
/// </summary>
[Generator]
public class NsisPluginExportSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
    }
}
