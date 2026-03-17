using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Export;

internal class Parser
{
    public const string NsisActionAttributeMetadataName = "NsisPlugin.NsisActionAttribute";
    private const string FromVariableAttributeName = "NsisPlugin.FromVariableAttribute";
    private const string ToVariableAttributeName = "NsisPlugin.ToVariableAttribute";

    private readonly HashSet<string> _entryPoints = [];
    public List<Diagnostic> Diagnostics { get; } = [];

    /// <summary>
    /// 解析方法语法上下文，生成类型规范列表
    /// </summary>
    public List<TypeGenerationSpec> Parse(ImmutableArray<GeneratorAttributeSyntaxContext> methodSyntaxContexts, CancellationToken token)
    {
        Dictionary<INamedTypeSymbol, List<MethodGenerationSpec>> typeDict = new(SymbolEqualityComparer.Default);
        foreach (var methodSyntaxContext in methodSyntaxContexts)
        {
            token.ThrowIfCancellationRequested();
            if (methodSyntaxContext.TargetSymbol is not IMethodSymbol method) continue;
            // 检查方法是否合格
            if (!method.IsEligible(out var reason))
            {
                Diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.MethodNotEligible, method.Locations.FirstOrDefault(), method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat), reason));
                continue;
            }

            var methodSpec = ParseMethod(method, methodSyntaxContext.Attributes);
            if (methodSpec.Actions.Count == 0) continue;

            if (!typeDict.TryGetValue(method.ContainingType, out var methodList)) typeDict[method.ContainingType] = methodList = [];
            methodList.Add(methodSpec);
        }

        var result = typeDict.Where(td => td.Value.Count > 0).Select(td =>
        {
            var namespaceName = td.Key.ContainingNamespace is { IsGlobalNamespace: false } ns ? ns.ToDisplayString() : null;
            return new TypeGenerationSpec(namespaceName, new TypeRef(td.Key), td.Value.ToImmutableEquatableArray());
        }).ToList();
        return result;
    }

    /// <summary>
    /// 解析方法，生成 MethodGenerationSpec
    /// </summary>
    private MethodGenerationSpec ParseMethod(IMethodSymbol method, ImmutableArray<AttributeData> attributes)
    {
        var returnSpec = ParseReturn();
        var parameterSpecs = method.Parameters.Select(ParseParameter).ToImmutableEquatableArray();
        var actionSpecs = attributes.Where(ad => ad.AttributeClass?.ToDisplayString() == NsisActionAttributeMetadataName)
            .Select(ad => ParseAction(ad, method.Name)).Where(ags => ags is not null).Select(ags => ags!).ToImmutableEquatableArray();
        return new(method.Name, returnSpec, parameterSpecs, actionSpecs);

        // 解析方法的返回值
        ReturnGenerationSpec ParseReturn()
        {
            // 没有特性
            var returnType = new TypeRef(method.ReturnType);
            if (method.GetReturnTypeAttributes().FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == ToVariableAttributeName) is not AttributeData toVariableAttr) return new(returnType, null);

            // 有特性但没有返回值
            if (returnType.SpecialType is SpecialType.System_Void) Diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.MissingReturnTypeWithToVariable, method.Locations.FirstOrDefault(), method.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));

            Debug.Assert(toVariableAttr.ConstructorArguments.Length == 1);
            var toVariable = (NsVariable)toVariableAttr.ConstructorArguments.FirstOrDefault().Value!;
            return new(returnType, toVariable);
        }

        // 解析方法的参数
        static ParameterGenerationSpec ParseParameter(IParameterSymbol parameter)
        {
            if (parameter.GetAttributes().FirstOrDefault(ad => ad.AttributeClass?.ToDisplayString() == FromVariableAttributeName) is not AttributeData fromVariableAttr) return new(new TypeRef(parameter.Type), parameter.Name, null);

            Debug.Assert(fromVariableAttr.ConstructorArguments.Length == 1);
            var fromVariable = (NsVariable)fromVariableAttr.ConstructorArguments.FirstOrDefault().Value!;
            return new(new TypeRef(parameter.Type), parameter.Name, fromVariable);
        }
    }

    /// <summary>
    /// 解析方法上的 NsisActionAttribute，生成 ActionGenerationSpec
    /// </summary>
    private ActionGenerationSpec? ParseAction(AttributeData attribute, string methodName)
    {
        var (entryPointFormat, encoding) = ParseNsisActionAttribute(attribute);

        // 格式化入口名称
        string entryPoint;
        try { entryPoint = string.Format(entryPointFormat, methodName); }
        catch (Exception ex)
        {
            Diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.InvalidEntryPointFormat, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation(), entryPointFormat, ex.Message));
            return null;
        }

        // 入口名称不合法 || 是保留关键字
        if (!SyntaxFacts.IsValidIdentifier(entryPoint) || SyntaxFacts.IsReservedKeyword(SyntaxFacts.GetKeywordKind(entryPoint)))
        {
            Diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.InvalidEntryPointName, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation(), entryPoint));
            return null;
        }

        // 入口名称冲突
        if (!_entryPoints.Add(entryPoint))
        {
            Diagnostics.Add(Diagnostic.Create(DiagnosticDescriptors.ActionEntryPointConflict, attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation(), entryPoint));
            return null;
        }

        return new ActionGenerationSpec(entryPoint, encoding);


        static (string, Encodings) ParseNsisActionAttribute(AttributeData attribute)
        {
            Debug.Assert(attribute.ConstructorArguments.Length == 1);
            var entryPointFormat = attribute.ConstructorArguments.FirstOrDefault().Value as string ?? "{0}";
            var encoding = (Encodings)(attribute.NamedArguments.FirstOrDefault(kv => kv.Key == nameof(NsisActionAttribute.Encoding)).Value.Value ?? Encodings.Undefined);
            return (entryPointFormat, encoding);
        }
    }
}
