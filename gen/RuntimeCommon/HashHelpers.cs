// ReSharper disable once CheckNamespace

namespace System.Numerics.Hashing;

/// <summary>
/// <seealso href="https://github.com/dotnet/runtime/blob/e9ec642ee7ab11ffe1b8e767809fe60dd4a45b82/src/libraries/System.Private.CoreLib/src/System/Numerics/Hashing/HashHelpers.cs">to source</seealso>
/// </summary>
internal static class HashHelpers
{
    public static int Combine(int h1, int h2)
    {
        var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
        return ((int)rol5 + h1) ^ h2;
    }
}
