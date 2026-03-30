#if !NET5_0_OR_GREATER
namespace System.Runtime.CompilerServices;

/// <summary>
/// Support the [ModuleInitializer]
///
/// <seealso href="https://learn.microsoft.com/en-us/dotNet/API/system.runtime.compilerservices.moduleinitializerattribute">to docs</seealso>
/// <seealso href="https://github.com/dotnet/dotnet/blob/d70206844a95b337601237466bfc6cbb7d52d6d4/src/runtime/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/NullableAttribute.cs">to source</seealso>
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal class ModuleInitializerAttribute : Attribute { }
#endif
