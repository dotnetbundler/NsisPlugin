using static NsisPlugin.SourceGeneration.Tests.Helper.AssertHelper;
using static NsisPlugin.SourceGeneration.Tests.Helper.CompilationHelper;

namespace NsisPlugin.SourceGeneration.Tests;

public class ExportSourceGeneratorGeneratableCasesTests
{
    // 快照目录
    private const string SnapshotsDirectory = "ExportSnapshots";

    [Fact]
    public Task DefaultEntryPoint()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class BasicCases
                                  {
                                      [NsisAction]
                                      public static void Ping() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        // 验证源编译诊断
        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        // 验证源生成器诊断
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        // 验证生成源编译诊断
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        // 生成源快照验证
        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task WithParameters_Return_ToVariable_AndEncoding()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class FullFlowCases
                                  {
                                      [NsisAction("sum_{0}")]
                                      [NsisAction("sum_{0}A", Encoding = NsEncoding.Ansi)]
                                      [NsisAction("sum_{0}U", Encoding = NsEncoding.Unicode)]
                                      [return: ToVariable(NsVariable.Inst0)]
                                      public static int Add([FromVariable(NsVariable.Inst2)] int fromVar, int fromStack, StackT stack, Variables vars, ExtraParameters extra) => fromVar + fromStack;
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task ExportClassName_For_GlobalNamespace_NestedType()
    {
        const string source = """
                              using NsisPlugin;

                              public static class Outer
                              {
                                  public static class Inner
                                  {
                                      [NsisAction("Do")]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task NullFormat_FallbackToMethodName()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class NullFormatCases
                                  {
                                      [NsisAction(null)]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task PartialInvalidMethod_StillGenerates()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class MixedEligibilityCases
                                  {
                                      [NsisAction]
                                      private static void Hidden() { }

                                      [NsisAction("ok")]
                                      public static void Visible() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task VoidReturnWithToVariable_ReportWarning()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class ReturnCases
                                  {
                                      [NsisAction]
                                      [return: ToVariable(NsVariable.Inst0)]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN102");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task EntryPointConflicts_PreserveUniqueActions()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class ConflictCases
                                  {
                                      [NsisAction("A")]
                                      [NsisAction("A")]
                                      [NsisAction("B")]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN121");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task Entry_Point_Conflicts_Between_Types_Retain_The_First_One()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
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
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN121");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task PartialInvalidActions_StillGenerates1()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class MixedActionCases
                                  {
                                      [NsisAction("ok")]
                                      [NsisAction("ok{0")]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN122");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

#if NET
        return VerifySnapshot(driver, SnapshotsDirectory);
#else
        // .net framework 不验证 NSPGEN122 的快照，因为诊断输出不完全一致
        return Task.CompletedTask;
#endif
    }

    [Fact]
    public Task PartialInvalidActions_StillGenerates2()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class MixedActionCases
                                  {
                                      [NsisAction("ok")]
                                      [NsisAction("1bad")]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN123");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return VerifySnapshot(driver, SnapshotsDirectory);
    }

    [Fact]
    public Task Order_Stability_In_Case_Of_Invalidity_And_Conflicts()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public static class MixedActionDiagnostics
                                  {
                                      [NsisAction("Same")]
                                      [NsisAction("bad-name")]
                                      [NsisAction("Same")]
                                      [NsisAction("bad{0")]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN123", "NSPGEN121", "NSPGEN122");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

#if NET
        return VerifySnapshot(driver, SnapshotsDirectory);
#else
        // .net framework 不验证 NSPGEN122 的快照，因为诊断输出不完全一致
        return Task.CompletedTask;
#endif
    }
}
