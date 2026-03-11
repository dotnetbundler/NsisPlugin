using Microsoft.CodeAnalysis;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Diagnostics;

internal static class ExportDiagnostics
{
    private const string Category = "NsisPlugin.SourceGeneration";

    // 方法不可用
    private static readonly DiagnosticDescriptor _methodNotEligible = new(
        "NSISPLUGINGEN101",
        "NsisAction target is not eligible",
        "NsisAction target '{0}' is skipped because {1}",
        Category,
        DiagnosticSeverity.Info,
        true);

    // 冲突的入口点
    private static readonly DiagnosticDescriptor _actionEntryPointConflict = new(
        "NSISPLUGINGEN102",
        "Conflicting entry point",
        "The entry point '{0}' is used by multiple methods, which is not supported. Consider specifying unique entry points using the EntryPoint property of the NsisAction attribute.",
        Category,
        DiagnosticSeverity.Warning,
        true);

    private static string GetReasonMessage(ExportMethodSkipReason reason) => reason switch
    {
        ExportMethodSkipReason.MethodIsNotStatic => "it is not static",
        ExportMethodSkipReason.MethodIsAbstract => "it is abstract",
        ExportMethodSkipReason.MethodIsGeneric => "it is generic",
        ExportMethodSkipReason.ContainingTypeIsGeneric => "its containing type is generic",
        ExportMethodSkipReason.ContainsRefKindParameter => "it has ref, out, or in parameters",
        ExportMethodSkipReason.AccessibilityNotSupported => "its accessibility is not public or internal",
        _ => "it does not satisfy export requirements"
    };

    /// <summary>
    /// 创建方法不可用的诊断信息
    /// </summary>
    public static DiagnosticInfo CreateMethodNotEligible(string methodDisplayName, ExportMethodSkipReason reason, Location? location) => DiagnosticInfo.Create(_methodNotEligible, location, [methodDisplayName, GetReasonMessage(reason)]);

    /// <summary>
    /// 创建导出入口点冲突的诊断信息
    /// </summary>
    public static DiagnosticInfo CreateActionEntryPointConflict(string entryPoint, Location? location) => DiagnosticInfo.Create(_actionEntryPointConflict, location, [entryPoint]);
}

internal enum ExportMethodSkipReason
{
    MethodIsNotStatic,
    MethodIsAbstract,
    MethodIsGeneric,
    ContainingTypeIsGeneric,
    ContainsRefKindParameter,
    AccessibilityNotSupported
}
