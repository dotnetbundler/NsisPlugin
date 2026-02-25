using System.Runtime.CompilerServices;

namespace NsisPlugin.SourceGeneration.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Init() => VerifySourceGenerators.Initialize();
}
