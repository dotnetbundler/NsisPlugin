using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Model;

internal readonly struct ExportMethodParseResult(ExportMethodSpec? exportMethodSpec, IEnumerable<DiagnosticInfo> diagnostics)
{
    private readonly ImmutableEquatableArray<DiagnosticInfo>? _diagnostics = diagnostics.ToImmutableEquatableArray();

    public ExportMethodSpec? ExportMethodSpec { get; } = exportMethodSpec;
    public ImmutableEquatableArray<DiagnosticInfo> Diagnostics => _diagnostics ?? ImmutableEquatableArray<DiagnosticInfo>.Empty;
}

internal readonly struct ExportParseResult(IEnumerable<ExportTypeSpec> types, IEnumerable<DiagnosticInfo> diagnostics)
{
    private readonly ImmutableEquatableArray<ExportTypeSpec>? _types = types.ToImmutableEquatableArray();
    private readonly ImmutableEquatableArray<DiagnosticInfo>? _diagnostics = diagnostics.ToImmutableEquatableArray();

    public ImmutableEquatableArray<ExportTypeSpec> Types => _types ?? ImmutableEquatableArray<ExportTypeSpec>.Empty;
    public ImmutableEquatableArray<DiagnosticInfo> Diagnostics => _diagnostics ?? ImmutableEquatableArray<DiagnosticInfo>.Empty;
}
