using Microsoft.CodeAnalysis;

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
    }
}
