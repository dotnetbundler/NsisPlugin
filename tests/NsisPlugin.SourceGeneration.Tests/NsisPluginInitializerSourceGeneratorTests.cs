using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NsisPlugin.SourceGeneration.Tests.Helper;

namespace NsisPlugin.SourceGeneration.Tests;

/// <summary>
/// NSIS插件-模块初始化器源代码生成器测试
/// </summary>
public class NsisPluginInitializerSourceGeneratorTests
{
    /// <summary>
    /// 该测试应成功生成，需要满足以下条件
    /// <list type="bullet">
    ///     <item>语言版本 C# 9.0 及以上</item>
    ///     <item>编译环境包含基础类库引用（如 System.Runtime.CompilerServices.ModuleInitializerAttribute）</item>
    ///     <item>生成器属性配置启用自动生成（AutoGenerateNsisPluginInitializer=true）</item>
    /// </list>
    /// </summary>
    /// <param name="languageVersion">语言版本</param>
    /// <param name="hasReferences">是否包含基础类库引用</param>
    /// <param name="properties">生成器属性配置</param>
    [Theory]
    [InlineData(LanguageVersion.CSharp9, true, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp10, true, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp12, true, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp14, true, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.Latest, true, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    public Task Should_GenerateCode(LanguageVersion languageVersion, bool hasReferences, IEnumerable<string> properties)
    {
        // 创建编译环境
        var compilation = CreateCompilation(languageVersion, hasReferences ? Helpers.GetCurrentReferences() : null);

        // 运行源生成器
        var driver = Helpers.CreateGeneratorDriver<NsisPluginInitializerSourceGenerator>(languageVersion, properties);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // 配置 Verify 以使用特定的目录和文件名存储快照
        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(NsisPluginInitializerSourceGeneratorTests)}.{nameof(Should_GenerateCode)}");
        settings.DisableRequireUniquePrefix();

        // 验证生成了代码快照
        return Verify(driver, settings);
    }

    /// <summary>
    /// 该测试不应成功生成，以下任何条件不满足时都不应生成代码：
    /// <list type="bullet">
    ///     <item>语言版本低于 C# 9.0</item>
    ///     <item>编译环境缺少基础类库引用（如 System.Runtime.CompilerServices.ModuleInitializerAttribute）</item>
    ///     <item>生成器属性配置未启用自动生成（AutoGenerateNsisPluginInitializer=true）</item>
    /// </list>
    /// </summary>
    /// <param name="languageVersion">语言版本</param>
    /// <param name="hasReferences">是否包含基础类库引用</param>
    /// <param name="properties">生成器属性配置</param>
    [Theory]
    [InlineData(LanguageVersion.CSharp8, true, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp9, false, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp9, true, new[] { "AutoGenerateNsisPluginInitializer=false" })]
    [InlineData(LanguageVersion.CSharp14, false, new[] { "AutoGenerateNsisPluginInitializer=false" })]
    public void Should_NotGenerateCode(LanguageVersion languageVersion, bool hasReferences, IEnumerable<string> properties)
    {
        var compilation = CreateCompilation(languageVersion, hasReferences ? Helpers.GetCurrentReferences() : null);
        var driver = Helpers.CreateGeneratorDriver<NsisPluginInitializerSourceGenerator>(languageVersion, properties);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // 验证生成器没有输出任何代码文件
        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.GeneratedTrees);

        // 验证没有报错或异常产生
        Assert.Empty(runResult.Diagnostics);
    }


    /// <summary>
    /// 辅助方法：根据指定的 C# 语言版本和额外的程序集引用创建编译环境
    /// </summary>
    /// <param name="languageVersion">语言版本</param>
    /// <param name="references">程序集引用</param>
    private static CSharpCompilation CreateCompilation(LanguageVersion languageVersion, IEnumerable<MetadataReference>? references)
    {
        // 创建 C# 编译环境
        // 这里我们不需要实际的源代码内容，因为 NsisPluginInitializerSourceGenerator 主要关注语言版本和引用
        // 只有在满足一下条件时才会生成代码：
        //  1. LanguageVersion >= C# 9.0
        //  2. Compilation 中存在 System.Runtime.CompilerServices.ModuleInitializerAttribute 的定义
        var syntaxTree = CSharpSyntaxTree.ParseText("", new CSharpParseOptions(languageVersion), cancellationToken: TestContext.Current.CancellationToken);

        return CSharpCompilation.Create("Tests", [syntaxTree], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
