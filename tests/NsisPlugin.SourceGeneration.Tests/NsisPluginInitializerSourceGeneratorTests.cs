using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NsisPlugin.SourceGeneration.Tests;

/// <summary>
/// NSIS插件-模块初始化器源代码生成器测试
/// </summary>
public class NsisPluginInitializerSourceGeneratorTests
{
    /// <summary>
    /// 满足所有条件（C# 9.0，包含基础类库引用）<br/>
    /// 成功生成代码
    /// </summary>
    [Fact]
    public Task Should_GenerateCode_When_CSharp9AndReferencesExist()
    {
        var compilation = CreateCompilation(LanguageVersion.CSharp9, TestHelper.GetCurrentReferences());

        // 运行源生成器
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new NsisPluginInitializerSourceGenerator());
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // 验证生成了代码快照
        return Verify(driver).UseDirectory("Snapshots");
    }

    /// <summary>
    /// 不满足引用条件（C# 9.0，缺少基础类库引用）<br/>
    /// 不生成代码
    /// </summary>
    [Fact]
    public void Should_NotGenerateCode_When_CSharp9ButNoReferences()
    {
        var compilation = CreateCompilation(LanguageVersion.CSharp9, null);

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new NsisPluginInitializerSourceGenerator());
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        // 验证生成器没有输出任何代码文件
        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.GeneratedTrees);

        // 验证没有报错或异常产生
        Assert.Empty(runResult.Diagnostics);
    }

    /// <summary>
    /// 不满足语言版本条件（C# 8.0，包含基础类库引用）<br/>
    /// 不生成代码
    /// </summary>
    [Fact]
    public void Should_NotGenerateCode_When_CSharp8EvenWithReferences()
    {
        var compilation = CreateCompilation(LanguageVersion.CSharp8, TestHelper.GetCurrentReferences());

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new NsisPluginInitializerSourceGenerator());
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
