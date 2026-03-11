using SourceGenerators;

namespace NsisPlugin.SourceGeneration;

internal readonly struct InitializerParseResult(params IEnumerable<DiagnosticInfo> diagnostics)
{
    public bool ShouldGenerate => Diagnostics.Count == 0;

    public ImmutableEquatableArray<DiagnosticInfo> Diagnostics { get; } = diagnostics.ToImmutableEquatableArray();
}
