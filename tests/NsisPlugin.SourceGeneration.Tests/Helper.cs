using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NsisPlugin.SourceGeneration.Tests;

public static class Helper
{
    /// <summary>
    /// 获取当前运行环境的所有基础程序集引用
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<MetadataReference> GetCurrentReferences() => AppDomain.CurrentDomain.GetAssemblies()
        .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
        .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

    /// <summary>
    /// 创建一个包含指定语言版本生成器驱动
    /// </summary>
    /// <param name="languageVersion">语言版本</param>
    /// <typeparam name="T">增量生成器类型</typeparam>
    /// <returns>生成器驱动</returns>
    public static GeneratorDriver CreateGeneratorDriver<T>(LanguageVersion languageVersion) where T : IIncrementalGenerator, new() =>
        CSharpGeneratorDriver.Create([new T().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(default, true),
            parseOptions: new CSharpParseOptions(languageVersion)
        );
}
