using System.Runtime.InteropServices;

namespace NsisPlugin.NsisApi;

/// <summary>
/// NSIS extra parameters structure
/// Used for interop with the native NSIS/plugin host
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L66-L71">extra_parameters Source</seealso>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct ExtraParameters
{
    /// <summary>
    /// Pointer to ExecFlags
    /// </summary>
    public ExecFlags* ExecFlags;

    /// <summary>
    /// Execute code segment
    /// </summary>
    public delegate* unmanaged[Stdcall]<int, IntPtr, int> ExecuteCodeSegment;

    /// <summary>
    /// Validate filename
    /// </summary>
    public delegate* unmanaged[Stdcall]<IntPtr, void> ValidateFilename;

    /// <summary>
    /// Register plugin callback
    /// returns 0 on success, 1 if already registered and &lt;0 on error
    /// </summary>
    public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> RegisterPluginCallback;
}
