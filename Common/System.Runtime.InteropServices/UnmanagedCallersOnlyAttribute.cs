#if !NET5_0_OR_GREATER
#pragma warning disable CS0649
namespace System.Runtime.InteropServices;

/// <summary>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.unmanagedcallersonlyattribute">to docs</seealso>
/// <seealso href="https://github.com/dotnet/dotnet/blob/b0f34d51fccc69fd334253924abd8d6853fad7aa/src/runtime/src/libraries/System.Private.CoreLib/src/System/Runtime/InteropServices/UnmanagedCallersOnlyAttribute.cs">to source</seealso>
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class UnmanagedCallersOnlyAttribute : Attribute
{
    public Type[]? CallConvs;
    public string? EntryPoint;
}
#endif
