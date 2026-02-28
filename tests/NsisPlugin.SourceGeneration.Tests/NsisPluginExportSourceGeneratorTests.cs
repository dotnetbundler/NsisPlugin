using NsisPlugin.SourceGeneration.Tests.Helper;

namespace NsisPlugin.SourceGeneration.Tests;

public class NsisPluginExportSourceGeneratorTests
{
    [Fact]
    public Task Should_Generate_Wrapper()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class NsisPluginTest
                                  {
                                      [NsisAction("AddU", Encoding = Encodings.Unicode)]
                                      public static int Add(int v1, int v2) => v1 + v2;
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
}
