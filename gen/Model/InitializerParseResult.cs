using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Model;

internal readonly struct InitializerParseResult(IEnumerable<DiagnosticInfo> diagnostics)
{
    private readonly ImmutableEquatableArray<DiagnosticInfo>? _diagnostics = diagnostics.ToImmutableEquatableArray();

    public bool ShouldGenerate => Diagnostics.Count == 0;
    public ImmutableEquatableArray<DiagnosticInfo> Diagnostics => _diagnostics ?? ImmutableEquatableArray<DiagnosticInfo>.Empty;
}
