using System.Runtime.CompilerServices;

namespace NsisPlugin.SourceGeneration.Tests.Helper;

public static class ModuleInitializer
{
    [ModuleInitializer]
    internal static void Init() => VerifySourceGenerators.Initialize();
}
