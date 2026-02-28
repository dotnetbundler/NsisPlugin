using Microsoft.CodeAnalysis.CSharp;
using NsisPlugin.SourceGeneration.Tests.Helper;

namespace NsisPlugin.SourceGeneration.Tests;

public class NsisPluginExportSourceGeneratorTests
{
    [Fact]
    public Task Should_Generate_EntryPoints_And_Wrappers()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class EntryPointCases
                                  {
                                      [NsisAction]
                                      public static int DefaultEntry(int value) => value;

                                      [NsisAction("My_{0}_Entry", Encoding = Encodings.Ansi)]
                                      public static int FormatEntry(int value) => value;

                                      [NsisAction("LiteralEntry", Encoding = Encodings.Unicode)]
                                      public static int LiteralEntry(int value) => value;

                                      [NsisAction("Bad {0", Encoding = Encodings.Undefined)]
                                      public static int BadFormatEntry(int value) => value;

                                      [NsisAction("")]
                                      public static int EmptyEntry(int value) => value;

                                      [NsisAction("   ")]
                                      public static int WhitespaceEntry(int value) => value;
                                  }
                              }
                              """;

        var dummy = typeof(NsisActionAttribute);

        var compilation = Helpers.CreateCompilation(source, LanguageVersion.CSharp12, Helpers.GetCurrentReferences());
        var driver = Helpers.CreateGeneratorDriver<NsisPluginExportSourceGenerator>(LanguageVersion.CSharp12);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(NsisPluginExportSourceGeneratorTests)}.{nameof(Should_Generate_EntryPoints_And_Wrappers)}");
        settings.DisableRequireUniquePrefix();

        return Verify(driver, settings);
    }

    [Fact]
    public Task Should_Bind_Parameters_And_Returns()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class BindingCases
                                  {
                                      [NsisAction("Bind_{0}")]
                                      [return: ToVariable(NsVariable.InstR0)]
                                      public static string Combine(
                                          [FromVariable(NsVariable.Inst1)] int count,
                                          string name,
                                          int? optional,
                                          Variables variables,
                                          StackT stack,
                                          ExtraParameters extra)
                                      {
                                          _ = variables;
                                          _ = stack;
                                          _ = extra;
                                          return name + count + optional;
                                      }

                                      [NsisAction("VoidCase")]
                                      public static void Do([FromVariable(NsVariable.Inst2)] string value) { }

                                      [NsisAction("StackReturn")]
                                      public static int Add(int left, int right) => left + right;
                                  }
                              }
                              """;

        var dummy = typeof(NsisActionAttribute);

        var compilation = Helpers.CreateCompilation(source, LanguageVersion.CSharp12, Helpers.GetCurrentReferences());
        var driver = Helpers.CreateGeneratorDriver<NsisPluginExportSourceGenerator>(LanguageVersion.CSharp12);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(NsisPluginExportSourceGeneratorTests)}.{nameof(Should_Bind_Parameters_And_Returns)}");
        settings.DisableRequireUniquePrefix();

        return Verify(driver, settings);
    }

    [Fact]
    public Task Should_Handle_Multiple_Attributes_And_Duplicate_EntryPoints()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class MultiAttributeCases
                                  {
                                      [NsisAction("Dup")]
                                      [NsisAction("Dup")]
                                      [NsisAction("Fmt_{0}")]
                                      public static int Multi(int value) => value;
                                  }
                              }
                              """;

        var dummy = typeof(NsisActionAttribute);

        var compilation = Helpers.CreateCompilation(source, LanguageVersion.CSharp12, Helpers.GetCurrentReferences());
        var driver = Helpers.CreateGeneratorDriver<NsisPluginExportSourceGenerator>(LanguageVersion.CSharp12);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(NsisPluginExportSourceGeneratorTests)}.{nameof(Should_Handle_Multiple_Attributes_And_Duplicate_EntryPoints)}");
        settings.DisableRequireUniquePrefix();

        return Verify(driver, settings);
    }
}
