using SourceGenerators;

namespace NsisPlugin.SourceGeneration;

internal readonly struct ExportMethodParseResult(ExportMethodSpec? exportMethodSpec, params IEnumerable<DiagnosticInfo> diagnostics)
{
    public ExportMethodSpec? ExportMethodSpec { get; } = exportMethodSpec;
    public ImmutableEquatableArray<DiagnosticInfo> Diagnostics { get; } = diagnostics.ToImmutableEquatableArray();
}

internal readonly struct ExportParseResult(IEnumerable<ExportTypeSpec> types, IEnumerable<DiagnosticInfo> diagnostics)
{
    public ImmutableEquatableArray<ExportTypeSpec> Types { get; } = types.ToImmutableEquatableArray();

    public ImmutableEquatableArray<DiagnosticInfo> Diagnostics { get; } = diagnostics.ToImmutableEquatableArray();
}
