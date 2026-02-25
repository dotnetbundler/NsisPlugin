using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NsisPlugin.SourceGeneration.Tests;

public class Test
{
    [ModuleInitializer]
    internal static void Init() => VerifySourceGenerators.Initialize();

    [Fact]
    public Task GeneratorTest()
    {
        const string source = """
                              using Test;

                              public class Hello
                              {
                                    public void Say() => System.Console.WriteLine("Hello!");
                              }
                              """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Tests", [syntaxTree]);

        var generator = new NsisPluginSourceGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

        return Verify(driver);
    }
}
