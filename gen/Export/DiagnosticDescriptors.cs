using Microsoft.CodeAnalysis;

namespace NsisPlugin.SourceGeneration.Export;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor MethodNotEligible { get; } = new(
        "NSPGEN101",
        "Method is not eligible for export",
        "Source generation skips NsisAction target '{0}' because {1}",
        Constants.NsisPluginSourceGenerationName,
        DiagnosticSeverity.Warning,
        true);

    public static DiagnosticDescriptor ActionEntryPointConflict { get; } = new(
        "NSPGEN102",
        "Entry point conflicts with another export",
        "Source generation found duplicate entry point '{0}', specify unique entry points by setting NsisActionAttribute.EntryPoint",
        Constants.NsisPluginSourceGenerationName,
        DiagnosticSeverity.Warning,
        true);

    /// <summary>
    /// 检查方法是否满足导出条件，如果不满足则返回不满足的原因字符串
    /// </summary>
    public static bool IsEligible(this IMethodSymbol method, out string? reason)
    {
        reason = method switch
        {
            { IsStatic: false } => "it is not static",
            { IsAbstract: true } => "it is abstract",
            { IsGenericMethod: true } => "it is generic",
            { ContainingType: null or { IsGenericType: true } } => "its containing type is generic",
            { Parameters: var parameters } when parameters.Any(p => p.RefKind != RefKind.None) => "it has ref, out, or in parameters",
            { DeclaredAccessibility: not (Accessibility.Public or Accessibility.Internal) } => "its accessibility is not public or internal",
            _ => null
        };
        return reason is null;
    }
}
