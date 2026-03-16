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

    public static string? TryGetMethodNotEligibleReason(IMethodSymbol method)
    {
        if (!method.IsStatic) return "it is not static";
        if (method.IsAbstract) return "it is abstract";
        if (method.IsGenericMethod) return "it is generic";
        if (method.ContainingType is null || method.ContainingType.IsGenericType) return "its containing type is generic";
        if (method.Parameters.Any(p => p.RefKind != RefKind.None)) return "it has ref, out, or in parameters";
        return method.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal) ? "its accessibility is not public or internal" : null;
    }
}
