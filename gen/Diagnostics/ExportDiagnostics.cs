using Microsoft.CodeAnalysis;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Diagnostics;

internal static class ExportDiagnostics
{
    // 方法不可用
    private static readonly DiagnosticDescriptor _methodNotEligible = DiagnosticCatalog.CreateDescriptor(
        "NSPGEN101",
        "Method is not eligible for export",
        "Source generation skips NsisAction target '{0}' because {1}",
        DiagnosticSeverity.Warning);

    // 冲突的入口点
    private static readonly DiagnosticDescriptor _actionEntryPointConflict = DiagnosticCatalog.CreateDescriptor(
        "NSPGEN102",
        "Entry point conflicts with another export",
        "Source generation found duplicate entry point '{0}', specify unique entry points by setting NsisActionAttribute.EntryPoint",
        DiagnosticSeverity.Warning);

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
