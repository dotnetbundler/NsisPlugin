using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using Microsoft.CodeAnalysis;
using NsisPlugin.SourceGeneration.Diagnostics;
using NsisPlugin.SourceGeneration.Model;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Parser;

internal static class ExportParser
{
    public const string NsisActionAttributeMetadataName = "NsisPlugin.NsisActionAttribute";
    private const string FromVariableAttributeName = "NsisPlugin.FromVariableAttribute";
    private const string ToVariableAttributeName = "NsisPlugin.ToVariableAttribute";


    /// <summary>
    /// 解析方法结果，进行诊断信息收集、导出入口点冲突检查和按包含类型分组
    /// </summary>
    public static ExportParseResult ParseMethodResults(ImmutableArray<ExportMethodParseResult> methodParseResults, CancellationToken _)
    {
        var diagnostics = new List<DiagnosticInfo>();
        var entryPoints = new HashSet<string>();
        var typeDict = new Dictionary<INamedTypeSymbol, List<ExportMethodSpec>>(SymbolEqualityComparer.Default);

        foreach (var mpr in methodParseResults)
        {
            // 记录诊断信息
            diagnostics.AddRange(mpr.Diagnostics);

            // 检查导出入口点冲突
            if (mpr.ExportMethodSpec is not ExportMethodSpec ems) continue;
            List<ExportActionSpec> validActions = new(ems.Actions.Count);
            foreach (var action in ems.Actions)
            {
                if (entryPoints.Add(action.EntryPoint)) validActions.Add(action);
                else diagnostics.Add(ExportDiagnostics.CreateActionEntryPointConflict(action.EntryPoint, action.AttributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation()));
            }
            if (validActions.Count == 0) continue;

            // 按包含类型分组
            var methodSpec = new ExportMethodSpec(ems.Method, validActions);
            if (!typeDict.TryGetValue(ems.Method.ContainingType, out var typeExportMethodSpecs)) typeDict[ems.Method.ContainingType] = typeExportMethodSpecs = [];
            typeExportMethodSpecs.Add(methodSpec);
        }

        var typeSpecs = typeDict.Select(tg => new ExportTypeSpec(tg.Key, tg.Value));
        return new ExportParseResult(typeSpecs, diagnostics);
    }

    /// <summary>
    /// 解析方法
    /// </summary>
    public static ExportMethodParseResult ParseMethod(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        if (context.TargetSymbol is not IMethodSymbol method) return new ExportMethodParseResult(null, []);

        // 检查方法是否满足导出条件，如果不满足则生成诊断信息并跳过
        if (GetSkipReason(method) is ExportMethodSkipReason reason)
        {
            var diagnostic = ExportDiagnostics.CreateMethodNotEligible(method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), reason, method.Locations.FirstOrDefault());
            return new ExportMethodParseResult(null, [diagnostic]);
        }

        // 解析方法上的 [NsisAction] 特性，生成对应的导出动作信息
        var actions = context.Attributes
            .Where(static attribute => attribute.AttributeClass?.ToDisplayString() == NsisActionAttributeMetadataName)
            .Select(attribute => ParseAction(attribute, method));

        return new ExportMethodParseResult(new ExportMethodSpec(method, actions), []);

        // 获取方法不符合导出条件的原因
        static ExportMethodSkipReason? GetSkipReason(IMethodSymbol method)
        {
            if (!method.IsStatic) return ExportMethodSkipReason.MethodIsNotStatic;
            if (method.IsAbstract) return ExportMethodSkipReason.MethodIsAbstract;
            if (method.IsGenericMethod) return ExportMethodSkipReason.MethodIsGeneric;
            if (method.ContainingType is null || method.ContainingType.IsGenericType) return ExportMethodSkipReason.ContainingTypeIsGeneric;
            if (method.Parameters.Any(static p => p.RefKind != RefKind.None)) return ExportMethodSkipReason.ContainsRefKindParameter;
            if (method.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal)) return ExportMethodSkipReason.AccessibilityNotSupported;
            return null;
        }
    }

    /// <summary>
    /// 解析方法上的 [NsisAction] 特性，生成对应的导出动作信息
    /// </summary>
    private static ExportActionSpec ParseAction(AttributeData attribute, IMethodSymbol method)
    {
        var (entryPointFormat, encoding) = ParseNsisActionAttribute(attribute);
        var entryPoint = FormatEntryPoint(entryPointFormat, method.Name);

        return new ExportActionSpec(attribute, entryPoint, encoding);

        static (string, Encodings) ParseNsisActionAttribute(AttributeData attribute)
        {
            Debug.Assert(attribute.ConstructorArguments.Length == 1);
            var entryPointFormat = attribute.ConstructorArguments.FirstOrDefault().Value as string ?? "{0}";
            var encoding = (Encodings)(attribute.NamedArguments.FirstOrDefault(kv => kv.Key == nameof(NsisActionAttribute.Encoding)).Value.Value ?? Encodings.Undefined);
            return (entryPointFormat, encoding);
        }

        static string FormatEntryPoint(string format, string methodName)
        {
            try { return string.Format(CultureInfo.InvariantCulture, format, methodName); }
            catch (FormatException) { return methodName; }
        }
    }


    /// <summary>
    /// 尝试从参数上获取 [FromVariable] 特性及指定的变量
    /// </summary>
    public static bool TryGetFromVariableAttribute(IParameterSymbol parameter, out NsVariable? variable)
    {
        // 检查特性是否存在
        variable = null;
        var fromVariableAttr = parameter.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == FromVariableAttributeName);
        if (fromVariableAttr is null) return false;

        // 特性参数
        Debug.Assert(fromVariableAttr.ConstructorArguments.Length == 1);
        variable = (NsVariable)fromVariableAttr.ConstructorArguments.FirstOrDefault().Value!;
        return true;
    }

    /// <summary>
    /// 尝试从方法返回值上获取 [ToVariable] 特性及指定的变量
    /// </summary>
    public static bool TryGetToVariableAttribute(IMethodSymbol method, out NsVariable? variable)
    {
        // 检查特性是否存在
        variable = null;
        var toVariableAttr = method.GetReturnTypeAttributes().FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == ToVariableAttributeName);
        if (toVariableAttr is null) return false;

        // 特性参数
        Debug.Assert(toVariableAttr.ConstructorArguments.Length == 1);
        variable = (NsVariable)toVariableAttr.ConstructorArguments.FirstOrDefault().Value!;
        return true;
    }
}
