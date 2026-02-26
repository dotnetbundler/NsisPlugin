using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NsisPlugin.SourceGeneration.Tests;

/// <summary>
/// NSIS插件-模块初始化器源代码生成器测试
/// </summary>
public class NsisPluginInitializerSourceGeneratorTests
{
    /// <summary>
    /// 满足条件（C# 9.0 及以上，包含基础类库引用）<br/>
    /// 应该成功生成代码
    /// </summary>
    /// <param name="languageVersion">语言版本</param>
    /// <param name="hasReferences">是否包含基础类库引用</param>
    [Theory]
    [InlineData(LanguageVersion.CSharp9, true)]
    [InlineData(LanguageVersion.CSharp10, true)]
    [InlineData(LanguageVersion.CSharp12, true)]
    [InlineData(LanguageVersion.CSharp14, true)]
    [InlineData(LanguageVersion.Latest, true)]
    public Task Should_GenerateCode(LanguageVersion languageVersion, bool hasReferences)
    {
        // 创建编译环境
        var compilation = CreateCompilation(languageVersion, hasReferences ? Helper.GetCurrentReferences() : null);

        // 运行源生成器
        var driver = Helper.CreateGeneratorDriver<NsisPluginInitializerSourceGenerator>(languageVersion);
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
    /// 不满足条件（语言版本过低，或缺少基础类库引用）<br/>
    /// 不应该生成任何代码，并且没有报错或异常产生
    /// </summary>
    /// <param name="languageVersion">语言版本</param>
    /// <param name="hasReferences">是否包含基础类库引用</param>
    [Theory]
    [InlineData(LanguageVersion.CSharp9, false)]
    [InlineData(LanguageVersion.CSharp10, false)]
    [InlineData(LanguageVersion.CSharp12, false)]
    [InlineData(LanguageVersion.CSharp14, false)]
    [InlineData(LanguageVersion.Latest, false)]
    [InlineData(LanguageVersion.CSharp8, true)]
    [InlineData(LanguageVersion.CSharp7, true)]
    [InlineData(LanguageVersion.CSharp6, true)]
    public void Should_NotGenerateCode(LanguageVersion languageVersion, bool hasReferences)
    {
        var compilation = CreateCompilation(languageVersion, hasReferences ? Helper.GetCurrentReferences() : null);
        var driver = Helper.CreateGeneratorDriver<NsisPluginInitializerSourceGenerator>(languageVersion);
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
