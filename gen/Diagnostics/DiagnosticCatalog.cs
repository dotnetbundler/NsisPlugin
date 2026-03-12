using Microsoft.CodeAnalysis;

namespace NsisPlugin.SourceGeneration.Diagnostics;

internal static class DiagnosticCatalog
{
    private const string Category = "NsisPlugin.SourceGeneration";

    public static DiagnosticDescriptor CreateDescriptor(string id, string title, string messageFormat, DiagnosticSeverity severity) => new(id, title, messageFormat, Category, severity, true);
}
