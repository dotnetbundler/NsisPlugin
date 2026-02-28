using NsisPlugin.SourceGeneration.Tests.Helper;

namespace NsisPlugin.SourceGeneration.Tests;

public class NsisPluginExportSourceGeneratorTests
{
    /*入口函数包装生成应该实现功能
     * 1. 根据 NsisActionAttribute 生成对应的导出函数
     * 2. 获取参数
     *  1. 默认从 StackT 从上获取参数，按照参数顺序
     *  2. 如果参数有 FromVariableAttribute，则从 Variables 获取对应变量
     *  3. 如果参数有特殊类型（Variables、StackT、ExtraParameters），则直接传入 NsPlugin.Variables、NsPlugin.StackTop、NsPlugin.ExtraParameters
     * 3. 返回值
     *  1. 如果返回值是 void 不处理
     *  2. 返回值默认推送到 StackTop
     *  3. 如果返回值有 ToVariableAttribute，则设置到对应变量
     */


    [Fact]
    public Task Should_Generate_Wrapper()
    {
        const string source = """
                              using System.Threading.Tasks;
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class NsisPluginTest
                                  {
                                      [NsisAction("AddU", Encoding = Encodings.Unicode)]
                                      [NsisAction("AddA", Encoding = Encodings.Ansi)]
                                      [NsisAction("AddUndefined", Encoding = Encodings.Undefined)]
                                      public static int Add(int v1, int v2) => v1 + v2;

                                      [NsisAction(Encoding = Encodings.Unicode)]
                                      public static async Task<int> Add2(int v1, int v2) => v1 + v2;
                                  }
                              }
                              """;

        // 访问依赖项，确保生成器能够正确解析 NsisActionAttribute 和 Encodings 类型
        var dummy = typeof(NsisActionAttribute);

        var compilation = Helpers.CreateCompilation(source, references: Helpers.GetCurrentReferences());

        var driver = Helpers.CreateGeneratorDriver<NsisPluginExportSourceGenerator>();
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(NsisPluginExportSourceGeneratorTests)}.{nameof(Should_Generate_Wrapper)}");
        settings.DisableRequireUniquePrefix();

        return Verify(driver, settings);
    }

    [Fact]
    public Task Should_Handle_Parameters_And_Returns()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class NsisPluginSpecials
                                  {
                                      [NsisAction("Sum_{0}")]
                                      [return: ToVariable(NsVariable.InstR0)]
                                      public static int Sum(
                                          [FromVariable(NsVariable.Inst1)] int v1,
                                          int v2,
                                          Variables variables,
                                          StackT stack,
                                          ExtraParameters extra)
                                      {
                                          _ = variables;
                                          _ = stack;
                                          _ = extra;
                                          return v1 + v2;
                                      }

                                      [NsisAction("SetVar")]
                                      public static void SetVar([FromVariable(NsVariable.Inst2)] string value) { }
                                  }
                              }
                              """;

        var dummy = typeof(NsisActionAttribute);

        var compilation = Helpers.CreateCompilation(source, references: Helpers.GetCurrentReferences());

        var driver = Helpers.CreateGeneratorDriver<NsisPluginExportSourceGenerator>();
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(NsisPluginExportSourceGeneratorTests)}.{nameof(Should_Handle_Parameters_And_Returns)}");
        settings.DisableRequireUniquePrefix();

        return Verify(driver, settings);
    }
}
