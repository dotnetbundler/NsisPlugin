using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NsisPlugin.SourceGeneration.Tests.Helper;

internal static class Helpers
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
    /// <param name="properties">属性字符串数组</param>
    /// <typeparam name="T">增量生成器类型</typeparam>
    /// <returns>生成器驱动</returns>
    public static GeneratorDriver CreateGeneratorDriver<T>(LanguageVersion languageVersion, params IEnumerable<string> properties)
        where T : IIncrementalGenerator, new() =>
        CSharpGeneratorDriver.Create([new T().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(default, true),
            parseOptions: new CSharpParseOptions(languageVersion),
            optionsProvider: CreateOptionsProvider(properties)
        );

    /// <summary>
    /// 创建一个包含指定属性的测试分析器配置选项提供者<br/>
    /// 属性格式为 "key=value"，例如 "AutoGenerateNsisPluginInitializer=true"。<br/>
    /// 只会处理格式正确的属性，其他格式的属性将被忽略。
    /// </summary>
    /// <param name="properties">属性字符串数组，每个字符串应为 "key=value" 格式</param>
    /// <returns>测试分析器配置选项提供者</returns>
    public static TestAnalyzerConfigOptionsProvider CreateOptionsProvider(params IEnumerable<string> properties) => new(
        properties.Select(p => p.Split('='))
            .Where(p => p.Length == 2)
            .ToDictionary(kv => $"build_property.{kv[0]}", kv => kv[1])
    );
}

/// <summary>
/// 测试用的分析器配置选项提供者
/// </summary>
/// <param name="properties">属性字典</param>
internal class TestAnalyzerConfigOptionsProvider(IDictionary<string, string> properties) : AnalyzerConfigOptionsProvider
{
    public override AnalyzerConfigOptions GlobalOptions { get; } = new SimpleOptions(properties);

    public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
    public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;

    private class SimpleOptions(IDictionary<string, string> props) : AnalyzerConfigOptions
    {
        public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => props.TryGetValue(key, out value);
    }
}
