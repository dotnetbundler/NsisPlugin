// ReSharper disable CheckNamespace

namespace System.Numerics.Hashing;

/// <summary>
/// <seealso href="https://github.com/dotnet/runtime/blob/8e05ac91f032e62605f58c3d5a042b16131756fd/src/libraries/System.Private.CoreLib/src/System/Numerics/Hashing/HashHelpers.cs">to source</seealso>
/// </summary>
internal static class HashHelpers
{
    public static int Combine(int h1, int h2)
    {
        // RyuJIT optimizes this to use the ROL instruction
        // Related GitHub pull request: https://github.com/dotnet/coreclr/pull/1830
        var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
        return ((int)rol5 + h1) ^ h2;
    }
}
