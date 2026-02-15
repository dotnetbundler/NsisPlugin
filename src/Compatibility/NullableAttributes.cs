// ReSharper disable once CheckNamespace

namespace System.Diagnostics.CodeAnalysis;

#if NETSTANDARD2_0
/// <summary>
/// 为 NotNullWhenAttribute 提供兼容性支持
/// <seealso href="https://github.com/dotnet/runtime/blob/3ea3efcbcbb12469ef8ec80797fe7054be1b0c30/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs#L62-L79">NotNullWhenAttribute Source</seealso>
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute
{
    public bool ReturnValue { get; } = returnValue;
}
#endif
