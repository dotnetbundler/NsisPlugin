using Microsoft.CodeAnalysis;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Export;

public sealed record ParseResult(ImmutableEquatableArray<TypeGenerationSpec> Types, ImmutableEquatableArray<Diagnostic> Diagnostics);

public sealed record TypeGenerationSpec(string? Namespace, TypeRef Type, ImmutableEquatableArray<MethodGenerationSpec> Methods);

public sealed record MethodGenerationSpec(string Name, ReturnGenerationSpec Return, ImmutableEquatableArray<ParameterGenerationSpec> Parameters, ImmutableEquatableArray<ActionGenerationSpec> Actions);

public sealed record ReturnGenerationSpec(TypeRef Type, NsVariable? ToVariable);

public sealed record ParameterGenerationSpec(TypeRef Type, string Name, NsVariable? FromVariable);

public sealed record ActionGenerationSpec(string EntryPoint, Encodings Encoding);
