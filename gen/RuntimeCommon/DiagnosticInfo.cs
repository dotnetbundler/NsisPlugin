using System.Numerics.Hashing;
using Microsoft.CodeAnalysis;

// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable CheckNamespace

namespace SourceGenerators;

/// <summary>
/// Descriptor for diagnostic instances using structural equality comparison.
/// Provides a work-around for https://github.com/dotnet/roslyn/issues/68291.
/// <seealso href="https://github.com/dotnet/runtime/blob/e9ec642ee7ab11ffe1b8e767809fe60dd4a45b82/src/libraries/Common/src/SourceGenerators/DiagnosticInfo.cs">to source</seealso>
/// </summary>
internal readonly struct DiagnosticInfo : IEquatable<DiagnosticInfo>
{
    public DiagnosticDescriptor Descriptor { get; private init; }
    public object?[] MessageArgs { get; private init; }
    public Location? Location { get; private init; }

    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, object?[]? messageArgs)
    {
        var trimmedLocation = location is null ? null : GetTrimmedLocation(location);

        return new DiagnosticInfo
        {
            Descriptor = descriptor,
            Location = trimmedLocation,
            MessageArgs = messageArgs ?? Array.Empty<object?>()
        };

        // Creates a copy of the Location instance that does not capture a reference to Compilation.
        static Location GetTrimmedLocation(Location location)
            => Location.Create(location.SourceTree?.FilePath ?? "", location.SourceSpan, location.GetLineSpan().Span);
    }

    public Diagnostic CreateDiagnostic()
        => Diagnostic.Create(Descriptor, Location, MessageArgs);

    public override readonly bool Equals(object? obj) => obj is DiagnosticInfo info && Equals(info);

    public readonly bool Equals(DiagnosticInfo other) => Descriptor.Equals(other.Descriptor) &&
                                                         MessageArgs.SequenceEqual(other.MessageArgs) &&
                                                         Location == other.Location;

    public override readonly int GetHashCode()
    {
        var hashCode = Descriptor.GetHashCode();
        foreach (var messageArg in MessageArgs)
        {
            hashCode = HashHelpers.Combine(hashCode, messageArg?.GetHashCode() ?? 0);
        }

        hashCode = HashHelpers.Combine(hashCode, Location?.GetHashCode() ?? 0);
        return hashCode;
    }
}
