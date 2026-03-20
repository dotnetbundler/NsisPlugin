// ReSharper disable once CheckNamespace

#if NETFRAMEWORK
namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// 为 NotNullIfNotNullAttribute 提供兼容性支持
    /// <seealso href="https://github.com/dotnet/runtime/blob/3ea3efcbcbb12469ef8ec80797fe7054be1b0c30/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs#L62-L79">to source</seealso>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class NotNullWhenAttribute(bool returnValue) : Attribute
    {
        public bool ReturnValue { get; } = returnValue;
    }

    /// <summary>
    /// 为 NotNullIfNotNullAttribute 提供兼容性支持
    /// <seealso href="https://github.com/dotnet/dotnet/blob/e921c3d511279a7ed37f83dbd346019c5b3ffc0e/src/runtime/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/NullableAttributes.cs#L81-L98">to source</seealso>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
    internal sealed class NotNullIfNotNullAttribute : Attribute
    {
        public NotNullIfNotNullAttribute(string parameterName) => ParameterName = parameterName;

        public string ParameterName { get; }
    }
}

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// <seealso href="https://github.com/dotnet/dotnet/blob/e921c3d511279a7ed37f83dbd346019c5b3ffc0e/src/runtime/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/ModuleInitializerAttribute.cs">to source</seealso>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ModuleInitializerAttribute : Attribute
    {
        public ModuleInitializerAttribute()
        {
        }
    }
}

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// <seealso href="https://github.com/dotnet/dotnet/blob/b0f34d51fccc69fd334253924abd8d6853fad7aa/src/runtime/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/UnmanagedCallersOnlyAttribute.cs">to source</seealso>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public UnmanagedCallersOnlyAttribute()
        {
        }
        public Type[]? CallConvs;

        public string? EntryPoint;
    }
}
#endif
