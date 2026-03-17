using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static NsisPlugin.SourceGeneration.Tests.Helper.CompilationHelper;

namespace NsisPlugin.SourceGeneration.Tests;

public class ExportSourceGeneratorTests
{
    [Fact]
    public Task Should_GenerateCode_For_ValidExports()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class ExportMethods
                              {
                                  [NsisAction("demo_{0}", Encoding = Encodings.Unicode)]
                                  [return: ToVariable(NsVariable.Inst0)]
                                  internal static int Add([FromVariable(NsVariable.Inst1)] int fromVar, int fromStack, StackT stack, Variables vars, ExtraParameters extra)
                                      => fromVar + fromStack;

                                  [NsisAction]
                                  internal static void Ping() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences(), parseOptions: CreateParseOptions(LanguageVersion.CSharp14));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.Diagnostics);
        Assert.Single(runResult.GeneratedTrees);

        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.UseFileName($"{nameof(ExportSourceGeneratorTests)}.{nameof(Should_GenerateCode_For_ValidExports)}");
        settings.DisableRequireUniquePrefix();

        return Verify(driver, settings);
    }

    [Fact]
    public Task Should_Report_MethodNotEligible_Diagnostics_Details()
    {
        var scenarios = new[]
        {
            new DiagnosticScenario("NotStatic", """
                                                using NsisPlugin;
                                                public class Demo { [NsisAction] public void A() { } }
                                                """),
            new DiagnosticScenario("GenericMethod", """
                                                    using NsisPlugin;
                                                    public static class Demo { [NsisAction] public static void A<T>() { } }
                                                    """),
            new DiagnosticScenario("GenericContainingType", """
                                                            using NsisPlugin;
                                                            public static class Demo<T> { [NsisAction] public static void A() { } }
                                                            """),
            new DiagnosticScenario("RefParameter", """
                                                   using NsisPlugin;
                                                   public static class Demo { [NsisAction] public static void A(ref int value) { } }
                                                   """),
            new DiagnosticScenario("PrivateMethod", """
                                                    using NsisPlugin;
                                                    public static class Demo { [NsisAction] private static void A() { } }
                                                    """)
        };

        var diagnostics = scenarios.Select(scenario =>
        {
            var compilation = CreateCompilation(scenario.Source, additionalReferences: GetExportReferences(), parseOptions: CreateParseOptions(LanguageVersion.CSharp14));
            GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
            driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

            var runResult = driver.GetRunResult();
            Assert.Single(runResult.Diagnostics);

            var diagnostic = runResult.Diagnostics[0];
            Assert.Equal("NSPGEN101", diagnostic.Id);
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
        settings.UseFileName($"{nameof(ExportSourceGeneratorTests)}.{nameof(Should_Report_MethodNotEligible_Diagnostics_Details)}");
        settings.DisableRequireUniquePrefix();

        return Verify(diagnostics, settings);
    }

    [Fact]
    public void Should_Report_EntryPointConflict_Diagnostics()
    {
        const string source = """
                              using NsisPlugin;

                              public static class Demo
                              {
                                  [NsisAction("Dup")]
                                  public static void First() { }

                                  [NsisAction("Dup")]
                                  public static void Second() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.Diagnostics);
        Assert.Equal("NSPGEN102", runResult.Diagnostics[0].Id);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("Dup_Gen", generatedSource);
        Assert.Contains("global::Demo.First();", generatedSource);
        Assert.DoesNotContain("global::Demo.Second();", generatedSource);
    }

    [Fact]
    public void Should_FallbackToMethodName_WhenEntryPointFormatInvalid()
    {
        const string source = """
                              using NsisPlugin;

                              public static class Demo
                              {
                                  [NsisAction("{0}{")]
                                  public static void DoWork() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.Diagnostics);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("DoWork_Gen", generatedSource);
    }

    [Fact]
    public void Should_NotGenerate_WhenAllActionsInvalid()
    {
        const string source = """
                              using NsisPlugin;

                              public class Demo
                              {
                                  [NsisAction]
                                  public void InstanceMethod() { }

                                  [NsisAction]
                                  private static void PrivateMethod() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.GeneratedTrees);
        Assert.Equal(2, runResult.Diagnostics.Length);
        Assert.All(runResult.Diagnostics, diagnostic => Assert.Equal("NSPGEN101", diagnostic.Id));
    }

    [Fact]
    public void Should_Report_MethodNotEligible_When_Abstract()
    {
        const string source = """
                              using NsisPlugin;

                              public interface IDemo
                              {
                                  [NsisAction]
                                  static abstract void Action();
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var diagnostics = driver.GetRunResult().Diagnostics;
        Assert.Single(diagnostics);
        Assert.Equal("NSPGEN101", diagnostics[0].Id);
        Assert.Contains("it is abstract", diagnostics[0].GetMessage());
    }

    [Fact]
    public void Should_Report_MethodNotEligible_When_OutParameter()
    {
        const string source = """
                              using NsisPlugin;

                              public static class Demo
                              {
                                  [NsisAction]
                                  public static void Action(out int value) => value = 0;
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var diagnostics = driver.GetRunResult().Diagnostics;
        Assert.Single(diagnostics);
        Assert.Equal("NSPGEN101", diagnostics[0].Id);
        Assert.Contains("ref, out, or in", diagnostics[0].GetMessage());
    }

    [Fact]
    public void Should_Report_MethodNotEligible_When_InParameter()
    {
        const string source = """
                              using NsisPlugin;

                              public static class Demo
                              {
                                  [NsisAction]
                                  public static void Action(in int value) { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var diagnostics = driver.GetRunResult().Diagnostics;
        Assert.Single(diagnostics);
        Assert.Equal("NSPGEN101", diagnostics[0].Id);
        Assert.Contains("ref, out, or in", diagnostics[0].GetMessage());
    }

    [Fact]
    public void Should_GenerateCode_For_Public_And_Internal_Methods()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class VisibilityCases
                              {
                                  [NsisAction("public_api")]
                                  public static void PublicApi() { }

                                  [NsisAction("internal_api")]
                                  internal static void InternalApi() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.Diagnostics);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("public_api_Gen", generatedSource);
        Assert.Contains("internal_api_Gen", generatedSource);
    }

    [Fact]
    public void Should_Handle_MultipleActions_With_PartialConflicts()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class MultiActions
                              {
                                  [NsisAction("A")]
                                  [NsisAction("A")]
                                  [NsisAction("B")]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.Diagnostics);
        Assert.Equal("NSPGEN102", runResult.Diagnostics[0].Id);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Equal(1, CountOccurrences(generatedSource, "public static void A_Gen"));
        Assert.Equal(1, CountOccurrences(generatedSource, "public static void B_Gen"));
    }

    [Fact]
    public void Should_UseUndefinedEncoding_WhenNotSpecified()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class EncodingCases
                              {
                                  [NsisAction]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.Diagnostics);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("CreateEncScope(global::NsisPlugin.Encodings.Undefined)", generatedSource);
    }

    [Fact]
    public void Should_GenerateCode_InGlobalNamespace_WithoutInvalidNamespaceLiteral()
    {
        const string source = """
                              using NsisPlugin;

                              public static class Demo
                              {
                                  [NsisAction]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.Diagnostics);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.DoesNotContain("namespace <global namespace>", generatedSource);
        Assert.Contains("public static class Demo_NsisExports", generatedSource);
    }

    [Fact]
    public Task Should_Report_EntryPointConflict_Diagnostics_Details()
    {
        var scenarios = new[]
        {
            new DiagnosticScenario("DuplicateEntryPointByLiteral", """
                                                                   using NsisPlugin;
                                                                   public static class Demo { [NsisAction("Dup")] public static void A() { } [NsisAction("Dup")] public static void B() { } }
                                                                   """),
            new DiagnosticScenario("DuplicateEntryPointByFormat", """
                                                                  using NsisPlugin;
                                                                  public static class Demo { [NsisAction("Same")] public static void A() { } [NsisAction("{0}")] public static void Same() { } }
                                                                  """)
        };

        var diagnostics = scenarios.Select(scenario =>
        {
            var compilation = CreateCompilation(scenario.Source, additionalReferences: GetExportReferences(), parseOptions: CreateParseOptions(LanguageVersion.CSharp14));
            GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
            driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

            var runResult = driver.GetRunResult();
            Assert.Single(runResult.Diagnostics);

            var diagnostic = runResult.Diagnostics[0];
            Assert.Equal("NSPGEN102", diagnostic.Id);
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
        settings.UseFileName($"{nameof(ExportSourceGeneratorTests)}.{nameof(Should_Report_EntryPointConflict_Diagnostics_Details)}");
        settings.DisableRequireUniquePrefix();

        return Verify(diagnostics, settings);
    }

    [Fact]
    public void Should_Ignore_ToVariableAttribute_OnVoidReturn()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class ReturnCases
                              {
                                  [NsisAction]
                                  [return: ToVariable(NsVariable.Inst0)]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.Diagnostics);
        Assert.Equal("NSPGEN103", runResult.Diagnostics[0].Id);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("global::Demo.ReturnCases.Work();", generatedSource);
        Assert.DoesNotContain("NsPluginExtensions.Set", generatedSource);
    }

    [Fact]
    public void Should_FallbackToMethodName_WhenEntryPointFormatIsNull()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class NullFormatCases
                              {
                                  [NsisAction(null)]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Empty(runResult.Diagnostics);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("EntryPoint = \"Work\"", generatedSource);
        Assert.Contains("public static void Work_Gen", generatedSource);
    }

    [Fact]
    public void Should_GenerateCode_For_EmptyEntryPoint()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class EmptyEntryPointCases
                              {
                                  [NsisAction("")]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.Diagnostics);
        Assert.Equal("NSPGEN104", runResult.Diagnostics[0].Id);
        Assert.Empty(runResult.GeneratedTrees);
    }

    [Fact]
    public void Should_GenerateCompilableCode_WhenEntryPointContainsHyphen()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class HyphenEntryPointCases
                              {
                                  [NsisAction("do-work")]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences(), parseOptions: CreateParseOptions(LanguageVersion.CSharp14));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics, TestContext.Current.CancellationToken);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id.StartsWith("CS", StringComparison.Ordinal));

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.Diagnostics);
        Assert.Equal("NSPGEN104", runResult.Diagnostics[0].Id);
        Assert.Empty(runResult.GeneratedTrees);
        Assert.DoesNotContain(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Should_GenerateCompilableCode_WhenEntryPointStartsWithDigit()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class NumericEntryPointCases
                              {
                                  [NsisAction("123Work")]
                                  public static void Work() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences(), parseOptions: CreateParseOptions(LanguageVersion.CSharp14));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics, TestContext.Current.CancellationToken);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id.StartsWith("CS", StringComparison.Ordinal));

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.Diagnostics);
        Assert.Equal("NSPGEN104", runResult.Diagnostics[0].Id);
        Assert.Empty(runResult.GeneratedTrees);
        Assert.DoesNotContain(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Should_GenerateUniqueWrapperNames_WhenEntryPointsNormalizeToSameIdentifier()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class EntryPointNormalizationCases
                              {
                                  [NsisAction("a-b")]
                                  public static void First() { }

                                  [NsisAction("a b")]
                                  public static void Second() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences(), parseOptions: CreateParseOptions(LanguageVersion.CSharp14));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics, TestContext.Current.CancellationToken);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id.StartsWith("CS", StringComparison.Ordinal));

        var runResult = driver.GetRunResult();
        Assert.Equal(2, runResult.Diagnostics.Length);
        Assert.All(runResult.Diagnostics, diagnostic => Assert.Equal("NSPGEN104", diagnostic.Id));
        Assert.Empty(runResult.GeneratedTrees);
        Assert.DoesNotContain(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Should_GenerateUniqueExportClassNames_ForNestedTypesWithSameName()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class OuterA
                              {
                                  public static class Inner
                                  {
                                      [NsisAction("A")]
                                      public static void Work() { }
                                  }
                              }

                              public static class OuterB
                              {
                                  public static class Inner
                                  {
                                      [NsisAction("B")]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences(), parseOptions: CreateParseOptions(LanguageVersion.CSharp14));
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics, TestContext.Current.CancellationToken);

        Assert.Empty(diagnostics);
        Assert.DoesNotContain(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken), diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources.Select(generatedSource => generatedSource.SourceText.ToString()).ToArray();
        Assert.Equal(2, generatedSources.Length);
        Assert.Contains(generatedSources, sourceText => sourceText.Contains("public static class OuterA_Inner_NsisExports", StringComparison.Ordinal));
        Assert.Contains(generatedSources, sourceText => sourceText.Contains("public static class OuterB_Inner_NsisExports", StringComparison.Ordinal));
    }

    [Fact]
    public void Should_NotGenerate_EmptyExportClass_WhenAllActionsConflictAcrossTypes()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo;

                              public static class FirstType
                              {
                                  [NsisAction("Dup")]
                                  public static void First() { }
                              }

                              public static class SecondType
                              {
                                  [NsisAction("Dup")]
                                  public static void Second() { }
                              }
                              """;

        var compilation = CreateCompilation(source, additionalReferences: GetExportReferences());
        GeneratorDriver driver = CreateGeneratorDriver<ExportSourceGenerator>(compilation);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        var runResult = driver.GetRunResult();
        Assert.Single(runResult.Diagnostics);
        Assert.Equal("NSPGEN102", runResult.Diagnostics[0].Id);
        Assert.Single(runResult.GeneratedTrees);

        var generatedSource = runResult.Results[0].GeneratedSources[0].SourceText.ToString();
        Assert.Contains("public static class FirstType_NsisExports", generatedSource);
        Assert.DoesNotContain("SecondType_NsisExports", generatedSource);
    }

    private static IEnumerable<MetadataReference> GetExportReferences() => GetReferences(
        typeof(NsPlugin),
        typeof(StackT),
        typeof(Variables),
        typeof(ExtraParameters))!;

    private readonly record struct DiagnosticScenario(string Name, string Source);

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
