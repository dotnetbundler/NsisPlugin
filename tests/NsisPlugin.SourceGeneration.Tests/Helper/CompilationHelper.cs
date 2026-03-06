using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NsisPlugin.SourceGeneration.Tests.Helper;

internal static class CompilationHelper
{
    private static readonly CSharpParseOptions _defaultParseOptions = CreateParseOptions();

    /// <summary>
    /// 创建 C# 解析选项
    /// </summary>
    /// <param name="languageVersion">语言版本，C#9 是源生成器所支持的最低语言版本</param>
    /// <param name="documentationMode">文档模式，默认为 Parse，表示解析 XML 文档注释但不生成文档</param>
    /// <returns>C# 解析选项</returns>
    public static CSharpParseOptions CreateParseOptions(LanguageVersion languageVersion = LanguageVersion.CSharp9, DocumentationMode documentationMode = DocumentationMode.Parse) => new(languageVersion, documentationMode);

    /// <summary>
    /// 获取指定类型所在程序集的引用
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    [return: NotNullIfNotNull(nameof(types))]
    public static IEnumerable<MetadataReference>? GetReferences(params IEnumerable<Type>? types) => types?
        .Select(t => t.Assembly)
        .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
        .Distinct()
        .Select(assembly => MetadataReference.CreateFromFile(assembly.Location));

    /// <summary>
    /// 创建一个包含指定属性的测试分析器配置选项提供者<br/>
    /// 属性格式为 "key=value"，例如 "AutoGenerateNsisPluginInitializer=true"。<br/>
    /// 只会处理格式正确的属性，其他格式的属性将被忽略。
    /// </summary>
    /// <param name="properties">属性字符串数组，每个字符串应为 "key=value" 格式</param>
    /// <returns>测试分析器配置选项提供者</returns>
    public static TestAnalyzerConfigOptionsProvider CreateAnalyzerConfigOptionsProvider(params IEnumerable<string> properties) => new(
        properties.Select(p => p.Split('=')).Where(p => p.Length == 2)
            .ToDictionary(kv => $"build_property.{kv[0]}", kv => kv[1])
    );

    /// <summary>
    /// 创建一个 C# 编译环境
    /// </summary>
    /// <param name="source"></param>
    /// <param name="includeBase"></param>
    /// <param name="additionalReferences"></param>
    /// <param name="assemblyName"></param>
    /// <param name="parseOptions"></param>
    /// <returns></returns>
    public static CSharpCompilation CreateCompilation(string source, bool includeBase = true, IEnumerable<MetadataReference>? additionalReferences = null, string assemblyName = "TestAssembly", CSharpParseOptions? parseOptions = null)
    {
        // 引用环境
        List<MetadataReference> references = [];
        if (additionalReferences is not null) references.AddRange(additionalReferences);
        if (includeBase)
        {
            references.AddRange(GetReferences(typeof(object), typeof(Enumerable)));
            references.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
        }

        // 语法树
        parseOptions ??= _defaultParseOptions;
        SyntaxTree[] syntaxTree = [CSharpSyntaxTree.ParseText(source, parseOptions)];

        // 创建编译环境
        return CSharpCompilation.Create(assemblyName, syntaxTree, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    /// <summary>
    /// 创建一个源生成器驱动
    /// </summary>
    /// <param name="compilation">编译环境，生成器将基于此环境进行分析和生成</param>
    /// <param name="generator">增量生成器实例，如果为 null 则会创建一个新的实例</param>
    /// <param name="properties">生成器配置属性字符串数组，每个字符串应为 "key=value" 格式，这些属性将被传递给生成器作为分析器配置选项</param>
    /// <typeparam name="T">增量生成器类型，必须实现 IIncrementalGenerator 接口并具有无参构造函数</typeparam>
    /// <returns></returns>
    public static CSharpGeneratorDriver CreateGeneratorDriver<T>(Compilation compilation, T? generator = default, params IEnumerable<string> properties) where T : IIncrementalGenerator, new()
    {
        generator ??= new();
        var parseOptions = compilation.SyntaxTrees.OfType<CSharpSyntaxTree>().Select(tree => tree.Options).FirstOrDefault() ?? _defaultParseOptions;
        var optionsProvider = CreateAnalyzerConfigOptionsProvider(properties);
        var generatorOptions = new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, true);

        return CSharpGeneratorDriver.Create([generator.AsSourceGenerator()],
            parseOptions: parseOptions,
            optionsProvider: optionsProvider,
            driverOptions: generatorOptions);
    }
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
