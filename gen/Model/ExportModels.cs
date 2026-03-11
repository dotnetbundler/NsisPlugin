using System.Diagnostics;
using System.Numerics.Hashing;
using Microsoft.CodeAnalysis;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration;

[DebuggerDisplay("ContainingType = {ContainingType}, Methods = {Methods.Count}")]
internal readonly struct ExportTypeSpec(INamedTypeSymbol containingType, IEnumerable<ExportMethodSpec> methods) : IEquatable<ExportTypeSpec>
{
    public INamedTypeSymbol ContainingType { get; } = containingType;
    public ImmutableEquatableArray<ExportMethodSpec> Methods { get; } = methods.ToImmutableEquatableArray();

    public bool Equals(ExportTypeSpec other) => SymbolEqualityComparer.Default.Equals(ContainingType, other.ContainingType) && Methods.Equals(other.Methods);
    public override bool Equals(object? obj) => obj is ExportTypeSpec other && Equals(other);
    public override int GetHashCode() => HashHelpers.Combine(SymbolEqualityComparer.Default.GetHashCode(ContainingType), Methods.GetHashCode());
}

[DebuggerDisplay("Method = {Method}, Actions = {Actions.Count}")]
internal readonly struct ExportMethodSpec(IMethodSymbol method, IEnumerable<ExportActionSpec> actions) : IEquatable<ExportMethodSpec>
{
    public IMethodSymbol Method { get; } = method;
    public ImmutableEquatableArray<ExportActionSpec> Actions { get; } = actions.ToImmutableEquatableArray();

    public bool Equals(ExportMethodSpec other) => SymbolEqualityComparer.Default.Equals(Method, other.Method) && Actions.Equals(other.Actions);
    public override bool Equals(object? obj) => obj is ExportMethodSpec other && Equals(other);
    public override int GetHashCode() => HashHelpers.Combine(SymbolEqualityComparer.Default.GetHashCode(Method), Actions.GetHashCode());
}

[DebuggerDisplay("EntryPoint = {EntryPoint}, Encoding = {Encoding}")]
internal readonly struct ExportActionSpec(AttributeData attributeData, string entryPoint, Encodings encoding) : IEquatable<ExportActionSpec>
{
    public AttributeData AttributeData { get; } = attributeData;
    public string EntryPoint { get; } = entryPoint;
    public Encodings Encoding { get; } = encoding;

    public bool Equals(ExportActionSpec other) => EntryPoint == other.EntryPoint && Encoding == other.Encoding;
    public override bool Equals(object? obj) => obj is ExportActionSpec other && Equals(other);
    public override int GetHashCode() => HashHelpers.Combine(EntryPoint.GetHashCode(), Encoding.GetHashCode());
}
