using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static NsisPlugin.SourceGeneration.Tests.Helper.CompilationHelper;

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
    ///     <item>编译环境包含 <see cref="ModuleInitializerAttribute">ModuleInitializer</see> 的定义</item>
    ///     <item>生成器属性配置启用自动生成（AutoGenerateNsisPluginInitializer=true）</item>
    /// </list>
    /// </summary>
    /// <param name="languageVersion">语言版本</param>
    /// <param name="referenceTypes">需要包含的类型引用，用于满足生成器对特定类型定义的检查</param>
    /// <param name="properties">生成器属性配置</param>
    [Theory]
    [InlineData(LanguageVersion.CSharp9, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp10, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp12, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp14, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.Latest, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    public Task Should_GenerateCode(LanguageVersion languageVersion, IEnumerable<Type>? referenceTypes, IEnumerable<string> properties)
    {
        // 创建编译环境
        var compilation = CreateCompilation("", false, GetReferences(referenceTypes), parseOptions: CreateParseOptions(languageVersion));

        // 运行源生成器
        GeneratorDriver driver = CreateGeneratorDriver<NsisPluginInitializerSourceGenerator>(compilation, properties: properties);
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
    [Theory]
    [InlineData(LanguageVersion.CSharp8, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp9, null, new[] { "AutoGenerateNsisPluginInitializer=true" })]
    [InlineData(LanguageVersion.CSharp9, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=false" })]
    [InlineData(LanguageVersion.CSharp14, null, new[] { "AutoGenerateNsisPluginInitializer=false" })]
    public void Should_NotGenerateCode(LanguageVersion languageVersion, IEnumerable<Type>? referenceTypes, IEnumerable<string> properties)
    {
        var compilation = CreateCompilation("", false, GetReferences(referenceTypes), parseOptions: CreateParseOptions(languageVersion));
        GeneratorDriver driver = CreateGeneratorDriver<NsisPluginInitializerSourceGenerator>(compilation, properties: properties);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // 验证生成器没有输出任何代码文件
        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.GeneratedTrees);

        // 验证没有报错或异常产生
        Assert.Empty(runResult.Diagnostics);
    }
}
