using Microsoft.CodeAnalysis;

namespace NsisPlugin.SourceGeneration.Tests;

public static class TestHelper
{
    /// <summary>
    /// 获取当前运行环境的所有基础程序集引用
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<MetadataReference> GetCurrentReferences() => AppDomain.CurrentDomain.GetAssemblies()
        .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
        .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));
}
