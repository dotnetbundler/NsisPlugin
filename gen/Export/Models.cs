using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Export;

internal sealed record TypeGenerationSpec(string? Namespace, TypeRef Type, ImmutableEquatableArray<MethodGenerationSpec> Methods);

internal sealed record MethodGenerationSpec(string Name, ReturnGenerationSpec Return, ImmutableEquatableArray<ParameterGenerationSpec> Parameters, ImmutableEquatableArray<ActionGenerationSpec> Actions);

internal sealed record ReturnGenerationSpec(TypeRef Type, NsVariable? ToVariable);

internal sealed record ParameterGenerationSpec(TypeRef Type, string Name, NsVariable? FromVariable);

internal sealed record ActionGenerationSpec(string EntryPoint, NsEncoding Encoding);
