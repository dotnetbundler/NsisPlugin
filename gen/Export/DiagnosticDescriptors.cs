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

    public static DiagnosticDescriptor ParameterTypeNotSupported { get; } = new(
        "NSPGEN102",
        "Parameter type is not supported for export",
        "The Parameter type '{0}' is not supported, ensure the type has implemented 'System.ISpanParsable<TSelf>'.",
        Constants.NsisPluginSourceGenerationName,
        DiagnosticSeverity.Warning,
        true);

    public static DiagnosticDescriptor MissingReturnTypeWithToVariable { get; } = new(
        "NSPGEN103",
        "Method with ToVariableAttribute must have a return value",
        "The method '{0}' is decorated with [ToVariable], but it returns void. Methods must return a value to be assigned to a variable.",
        Constants.NsisPluginSourceGenerationName,
        DiagnosticSeverity.Info,
        true);

    public static DiagnosticDescriptor ActionEntryPointConflict { get; } = new(
        "NSPGEN121",
        "Entry point conflicts with another export",
        "Source generation found duplicate entry point '{0}', specify unique entry points by setting NsisActionAttribute.EntryPoint",
        Constants.NsisPluginSourceGenerationName,
        DiagnosticSeverity.Warning,
        true);

    public static DiagnosticDescriptor InvalidEntryPointFormat { get; } = new(
        "NSPGEN122",
        "Invalid entry point format string",
        "The entry point format string '{0}' is invalid.",
        Constants.NsisPluginSourceGenerationName,
        DiagnosticSeverity.Warning,
        true);

    public static DiagnosticDescriptor InvalidEntryPointName { get; } = new(
        "NSPGEN123",
        "Invalid entry point name",
        "The entry point name '{0}' is not a valid C# identifier or contains characters not supported for export.",
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
            { DeclaredAccessibility: not (Accessibility.Public or Accessibility.Internal) } => "its accessibility is not public or internal",
            { Parameters: var parameters } when parameters.Any(p => p.RefKind != RefKind.None) => "it has ref, out, or in parameters",
            { ContainingType: null or { TypeKind: TypeKind.Error } or { IsImplicitClass: true } } => "it is not contained in a type", // 没有容器类 || 是隐式类
            { ContainingType.IsGenericType: true } => "its containing type is generic", // 父级中有一个类型是泛型 IsGenericType 就为 true
            not null when !IsAccessibility(method.ContainingType) => "it is not accessibility",
            _ => null
        };
        return reason is null;

        // 是否可访问
        static bool IsAccessibility(INamedTypeSymbol type)
        {
            for (; type != null; type = type.ContainingType)
                if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
                    return false;
            return true;
        }
    }
}
