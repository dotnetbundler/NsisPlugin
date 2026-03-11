using Microsoft.CodeAnalysis;
using static NsisPlugin.SourceGeneration.Tests.Helper.CompilationHelper;

namespace NsisPlugin.SourceGeneration.Tests;

public class ExportSourceGeneratorTests
{
    [Fact]
    public Task Should_Generate_EntryPoints_And_Wrappers()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class AA
                                  {
                                  public static class EntryPointCases
                                  {
                                      [NsisAction]
                                      public static int DefaultEntry(int? val,string? a) => value;

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
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetReferences(typeof(NsPlugin)));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(ExportSourceGeneratorTests)}.{nameof(Should_Generate_EntryPoints_And_Wrappers)}");
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

        var compilation = CreateCompilation(source, additionalReferences: GetReferences(typeof(NsPlugin)));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(ExportSourceGeneratorTests)}.{nameof(Should_Bind_Parameters_And_Returns)}");
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

        var compilation = CreateCompilation(source, additionalReferences: GetReferences(typeof(NsPlugin)));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(ExportSourceGeneratorTests)}.{nameof(Should_Handle_Multiple_Attributes_And_Duplicate_EntryPoints)}");
        settings.DisableRequireUniquePrefix();

        return Verify(driver, settings);
    }

    [Fact]
    public void Should_NotGenerate_When_Methods_Are_Ineligible()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class IneligibleCases
                                  {
                                      [NsisAction]
                                      public static void RefParameter(ref int value) { }

                                      [NsisAction]
                                      public static T Generic<T>(T value) => value;
                                  }

                                  public class NonStaticContainer
                                  {
                                      [NsisAction]
                                      public int InstanceMethod(int value) => value;
                                  }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetReferences(typeof(NsPlugin)));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.GeneratedTrees);
        Assert.Empty(runResult.Diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        Assert.Equal(3, runResult.Diagnostics.Length);
        foreach (var diagnostic in runResult.Diagnostics)
        {
            Assert.Equal("NSPGEN101", diagnostic.Id);
            Assert.Equal(DiagnosticSeverity.Info, diagnostic.Severity);
            // Assert.True(diagnostic.Location.IsInSource);
            // Assert.NotNull(diagnostic.Location.SourceTree);
            Assert.True(diagnostic.Location.GetLineSpan().StartLinePosition.Line >= 0);
        }

        var messages = runResult.Diagnostics.Select(static diagnostic => diagnostic.GetMessage()).OrderBy(static message => message).ToArray();
        Assert.Equal(3, messages.Length);

        Assert.Contains("IneligibleCases.Generic<T>(T)", messages[0], StringComparison.Ordinal);
        Assert.Contains("it is generic", messages[0], StringComparison.Ordinal);

        Assert.Contains("IneligibleCases.RefParameter(ref int)", messages[1], StringComparison.Ordinal);
        Assert.Contains("it has ref, out, or in parameters", messages[1], StringComparison.Ordinal);

        Assert.Contains("NonStaticContainer.InstanceMethod(int)", messages[2], StringComparison.Ordinal);
        Assert.Contains("it is not static", messages[2], StringComparison.Ordinal);
    }

    [Fact]
    public Task Should_Report_Ineligible_Diagnostics_With_Locations()
    {
        const string source = """
                              using NsisPlugin;

                              namespace NsisPluginTest
                              {
                                  public static class IneligibleCases
                                  {
                                      [NsisAction]
                                      public static void RefParameter(ref int value) { }

                                      [NsisAction]
                                      public static T Generic<T>(T value) => value;
                                  }

                                  public class NonStaticContainer
                                  {
                                      [NsisAction]
                                      public int InstanceMethod(int value) => value;
                                  }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetReferences(typeof(NsPlugin)));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var diagnostics = driver.GetRunResult().Diagnostics
            .OrderBy(static diagnostic => diagnostic.GetMessage(), StringComparer.Ordinal)
            .Select(static diagnostic =>
            {
                var lineSpan = diagnostic.Location.GetLineSpan();
                return new
                {
                    diagnostic.Id,
                    Severity = diagnostic.Severity.ToString(),
                    Message = diagnostic.GetMessage(),
                    Line = lineSpan.StartLinePosition.Line + 1,
                    Column = lineSpan.StartLinePosition.Character + 1
                };
            })
            .ToArray();

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(ExportSourceGeneratorTests)}.{nameof(Should_Report_Ineligible_Diagnostics_With_Locations)}");
        settings.DisableRequireUniquePrefix();

        return Verify(diagnostics, settings);
    }
}
