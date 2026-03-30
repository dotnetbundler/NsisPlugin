#if !NET5_0_OR_GREATER
using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Support the init keyword
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.isexternalinit">to docs</seealso>
/// <seealso href="https://github.com/dotnet/dotnet/blob/d70206844a95b337601237466bfc6cbb7d52d6d4/src/runtime/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/IsExternalInit.cs">to source</seealso>
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit { }
#endif
