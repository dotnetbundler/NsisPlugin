using Microsoft.CodeAnalysis.CSharp;
using static NsisPlugin.SourceGeneration.Tests.Helper.AssertHelper;
using static NsisPlugin.SourceGeneration.Tests.Helper.CompilationHelper;

namespace NsisPlugin.SourceGeneration.Tests;

public class ExportSourceGeneratorNonGeneratableCasesTests
{
    [Fact]
    public void NoNsisActionAttribute()
    {
        const string source = """
                              namespace Demo
                              {
                                  public static class NoAttributeCases
                                  {
                                      public static void Work() { }
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

        // 验证是否生成源
        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void NonStatic_Method()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class NonStaticMethodCases
                                  {
                                      [NsisAction]
                                      public void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void Abstract_Method()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public interface AbstractMethodCases
                                  {
                                      [NsisAction]
                                      static abstract void Work();
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation, CreateParseOptions(LanguageVersion.CSharp11));

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void Generic_Method()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class GenericMethodCases
                                  {
                                      [NsisAction]
                                      public static void Work<T>() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void Generic_ContainingType()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class GenericContainingTypeCasesA<T>
                                  {
                                      [NsisAction]
                                      public static void Work() { }
                                  }

                                  public class GenericContainingTypeCasesB<T>
                                  {
                                      public class GenericContainingTypeCases1
                                      {
                                          [NsisAction]
                                          public static void Work() { }
                                      }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101", "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void Private_Method()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class PrivateMethodCases
                                  {
                                      [NsisAction]
                                      private static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void Private_ContainingType()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class PrivateContainingTypeCasesA
                                  {
                                      private class PrivateContainingTypeCases1
                                      {
                                          [NsisAction]
                                          public static void Work() { }
                                      }
                                  }

                                  public class PrivateContainingTypeCasesB
                                  {
                                      private class PrivateContainingTypeCases1
                                      {
                                          public class PrivateContainingTypeCases2
                                          {
                                              [NsisAction]
                                              public static void Work() { }
                                          }
                                      }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101", "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void Not_In_ContainingType()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  [NsisAction]
                                  public static void Work() { }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken), "CS0116");
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken), "CS0116");

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void RefOutInParameter()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class RefOutInParameterCases
                                  {
                                      [NsisAction]
                                      public static void RefCase(ref int value) { }

                                      [NsisAction]
                                      public static void OutCase(out int value) => value = 0;

                                      [NsisAction]
                                      public static void InCase(in int value) { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN101", "NSPGEN101", "NSPGEN101");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void ActionEntryPointConflict()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class ActionEntryPointConflictCasesA
                                  {
                                      [NsisAction("Work")]
                                      public static void WorkOk() { }

                                      [NsisAction("Work")]
                                      public static void Work2() { }
                                  }

                                  public class ActionEntryPointConflictCasesB
                                  {
                                      [NsisAction("Work")]
                                      public static void Work1() { }

                                      [NsisAction("Work")]
                                      public static void Work2() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN121", "NSPGEN121", "NSPGEN121");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count() + 1, outputCompilation.SyntaxTrees.Count());
        Assert.Contains("WorkOk", outputCompilation.SyntaxTrees.Last().ToString());
    }

    [Fact]
    public void InvalidEntryPointFormat()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class InvalidEntryPointFormatCases
                                  {
                                      [NsisAction("{0}{")]
                                      public static void Work() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN122");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }

    [Fact]
    public void InvalidEntryPoint()
    {
        const string source = """
                              using NsisPlugin;

                              namespace Demo
                              {
                                  public class InvalidEntryPointCases
                                  {
                                      [NsisAction("")]
                                      public static void Empty() { }

                                      [NsisAction("do-work")]
                                      public static void Hyphen() { }

                                      [NsisAction("123work")]
                                      public static void StartsWithDigit() { }

                                      [NsisAction("class")]
                                      public static void ReservedKeyword() { }
                                  }
                              }
                              """;

        var driver = RunGeneratorsAndCompilation<ExportSourceGenerator>(source, out var sourceCompilation, out var generatorDiagnostics, out var outputCompilation);

        AssertDiagnosticIdsInOrder(sourceCompilation.GetDiagnostics(TestContext.Current.CancellationToken));
        AssertDiagnosticIdsInOrder(generatorDiagnostics, "NSPGEN123", "NSPGEN123", "NSPGEN123", "NSPGEN123");
        AssertDiagnosticIdsInOrder(outputCompilation.GetDiagnostics(TestContext.Current.CancellationToken));

        Assert.Equal(sourceCompilation.SyntaxTrees.Count(), outputCompilation.SyntaxTrees.Count());
    }
}
