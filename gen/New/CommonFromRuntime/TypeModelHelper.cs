using Microsoft.CodeAnalysis;

// ReSharper disable CheckNamespace

namespace SourceGenerators;

/// <summary>
/// <seealso href="https://github.com/dotnet/runtime/blob/9a2c40b4f7b710d164a13ef4b088ba309068d21d/src/libraries/Common/src/SourceGenerators/TypeModelHelper.cs">to source</seealso>
/// </summary>
internal static class TypeModelHelper
{
    public static List<ITypeSymbol>? GetAllTypeArgumentsInScope(this INamedTypeSymbol type)
    {
        if (!type.IsGenericType)
        {
            return null;
        }

        List<ITypeSymbol>? args = null;
        TraverseContainingTypes(type);
        return args;

        void TraverseContainingTypes(INamedTypeSymbol current)
        {
            if (current.ContainingType is INamedTypeSymbol parent)
            {
                TraverseContainingTypes(parent);
            }

            if (!current.TypeArguments.IsEmpty)
            {
                (args ??= new()).AddRange(current.TypeArguments);
            }
        }
    }

    public static string GetFullyQualifiedName(this ITypeSymbol type) => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}
