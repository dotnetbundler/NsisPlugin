using static NsisPlugin.SourceGeneration.Tests.Helper.AssertHelper;
using static NsisPlugin.SourceGeneration.Tests.Helper.CompilationHelper;

namespace NsisPlugin.SourceGeneration.Tests;

public class ExportSourceGeneratorGeneratableCasesTests
{
    // 独立模式和共享入口初始化模式的快照分开存储，避免相互干扰
    private const string IndependentSnapshotsDirectory = "IndependentExportSnapshots";
    private const string SharedEntryInitSnapshotsDirectory = "SharedEntryInitExportSnapshots";

    private static readonly string[] _useSharedEntryInitProperties = ["NsisUseSharedExportEntryInit=true"];


    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task DefaultEntryPoint(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);

        // 验证源编译诊断
        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        // 验证源生成器诊断
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        // 验证生成源编译诊断
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task WithParameters_Return_ToVariable_AndEncoding(bool useSharedEntryInit)
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

                                      [NsisAction(Encoding = NsEncoding.Ansi)]
                                      [return: ToVariable(NsVariable.Inst0)]
                                      public static int Work(StackT stack, Variables vars, ExtraParameters extra) => 123;
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task ExportClassName_For_GlobalNamespace_NestedType(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task NullFormat_FallbackToMethodName(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics);
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task PartialInvalidMethod_StillGenerates(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task VoidReturnWithToVariable_ReportWarning(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN102");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task EntryPointConflicts_PreserveUniqueActions(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN121");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task Entry_Point_Conflicts_Between_Types_Retain_The_First_One(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN121");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task PartialInvalidActions_StillGenerates1(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN122");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task PartialInvalidActions_StillGenerates2(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN123");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public Task Order_Stability_In_Case_Of_Invalidity_And_Conflicts(bool useSharedEntryInit)
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

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, properties: useSharedEntryInit ? _useSharedEntryInitProperties : []);


        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN123", "NSPGEN121", "NSPGEN122");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        return useSharedEntryInit ?
            VerifySharedEntryInitExportSnapshot(driver, SharedEntryInitSnapshotsDirectory) :
            VerifySnapshot(driver, IndependentSnapshotsDirectory);
    }
}
