using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static NsisPlugin.SourceGeneration.Tests.Helper.CompilationHelper;

namespace NsisPlugin.SourceGeneration.Tests;

/// <summary>
/// NSIS插件-模块初始化器源代码生成器测试
/// </summary>
public class InitializerSourceGeneratorTests
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
    [InlineData(LanguageVersion.CSharp14, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=TRUE" })]
    public Task Should_GenerateCode(LanguageVersion languageVersion, IEnumerable<Type>? referenceTypes, IEnumerable<string> properties)
    {
        // 创建编译环境
        var compilation = CreateCompilation("", false, GetReferences(referenceTypes), parseOptions: CreateParseOptions(languageVersion));

        // 运行源生成器
        GeneratorDriver driver = CreateGeneratorDriver<InitializerSourceGenerator>(compilation, properties: properties);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // 配置 Verify 以使用特定的目录和文件名存储快照
        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(InitializerSourceGeneratorTests)}.{nameof(Should_GenerateCode)}");
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
    [InlineData(LanguageVersion.CSharp8, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=true" }, "NSISPLUGINGEN002")]
    [InlineData(LanguageVersion.CSharp9, null, new[] { "AutoGenerateNsisPluginInitializer=true" }, "NSISPLUGINGEN003")]
    [InlineData(LanguageVersion.CSharp9, new[] { typeof(ModuleInitializerAttribute) }, new[] { "AutoGenerateNsisPluginInitializer=false" }, "NSISPLUGINGEN001")]
    [InlineData(LanguageVersion.CSharp14, null, new[] { "AutoGenerateNsisPluginInitializer=false" }, "NSISPLUGINGEN001")]
    [InlineData(LanguageVersion.CSharp14, new[] { typeof(ModuleInitializerAttribute) }, new string[0], "NSISPLUGINGEN001")]
    public void Should_NotGenerateCode(LanguageVersion languageVersion, IEnumerable<Type>? referenceTypes, IEnumerable<string> properties, string expectedDiagnosticId)
    {
        var compilation = CreateCompilation("", false, GetReferences(referenceTypes), parseOptions: CreateParseOptions(languageVersion));
        GeneratorDriver driver = CreateGeneratorDriver<InitializerSourceGenerator>(compilation, properties: properties);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.GeneratedTrees);

        // 验证有且仅有一个跳过原因诊断，且不包含错误级别诊断。
        Assert.Single(runResult.Diagnostics);
        Assert.DoesNotContain(runResult.Diagnostics, static d => d.Severity == DiagnosticSeverity.Error);
        Assert.Equal(DiagnosticSeverity.Info, runResult.Diagnostics[0].Severity);
        Assert.Equal(expectedDiagnosticId, runResult.Diagnostics[0].Id);
    }


    private readonly record struct SkipScenario(
        string Name,
        LanguageVersion LanguageVersion,
        IEnumerable<MetadataReference>? References,
        string[] Properties);

    /// <summary>
    /// 验证跳过诊断的详细信息是否在不同场景下保持一致。
    /// </summary>
    [Fact]
    public Task Should_Report_Skip_Diagnostics_Details()
    {
        var scenarios = new[]
        {
            new SkipScenario(
                "LanguageTooLow",
                LanguageVersion.CSharp8,
                GetReferences(typeof(ModuleInitializerAttribute)),
                ["AutoGenerateNsisPluginInitializer=true"]),
            new SkipScenario(
                "MissingModuleInitializerAttribute",
                LanguageVersion.CSharp9,
                GetReferences(null),
                ["AutoGenerateNsisPluginInitializer=true"]),
            new SkipScenario(
                "AutoGenerateDisabled",
                LanguageVersion.CSharp14,
                GetReferences(typeof(ModuleInitializerAttribute)),
                ["AutoGenerateNsisPluginInitializer=false"])
        };

        var diagnostics = scenarios.Select(scenario =>
        {
            var compilation = CreateCompilation("", false, scenario.References, parseOptions: CreateParseOptions(scenario.LanguageVersion));
            GeneratorDriver driver = CreateGeneratorDriver<InitializerSourceGenerator>(compilation, properties: scenario.Properties);
            driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

            var runResult = driver.GetRunResult();
            Assert.Single(runResult.Diagnostics);

            var diagnostic = runResult.Diagnostics[0];
            return new
            {
                Scenario = scenario.Name,
                diagnostic.Id,
                Severity = diagnostic.Severity.ToString(),
                Message = diagnostic.GetMessage(),
                HasSourceLocation = diagnostic.Location.IsInSource
            };
        }).ToArray();

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(InitializerSourceGeneratorTests)}.{nameof(Should_Report_Skip_Diagnostics_Details)}");
        settings.DisableRequireUniquePrefix();

        return Verify(diagnostics, settings);
    }
}
