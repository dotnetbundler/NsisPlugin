using System.Runtime.InteropServices;

namespace NsisPlugin.NsisApi;

/// <summary>
/// NSIS exec_flags_t Structure
/// All fields are 32-bit integers: when used as a boolean flag, they follow the convention 0 = false, non-zero = true
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L39-L57">exec_flags_t Source</seealso>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ExecFlags
{
    /// <summary>
    /// Auto-close flag
    /// </summary>
    public int Autoclose;

    /// <summary>
    /// Variable scope: User context = 0, Machine context = 1
    /// </summary>
    public int AllUserVar;

    /// <summary>
    /// Execution error flag
    /// </summary>
    public int ExecError;

    /// <summary>
    /// Abort flag
    /// </summary>
    public int Abort;

    /// <summary>
    /// Indicates whether a reboot is required
    /// </summary>
    public int ExecReboot;

    /// <summary>
    /// Indicates whether reboot has been invoked
    /// </summary>
    public int RebootCalled;

    /// <summary>
    /// Deprecated; retained for backward ABI/layout compatibility
    /// </summary>
    public int XxxCurInsttype;

    /// <summary>
    /// Plugin API/ABI version
    /// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L24-L25">NSISPIAPIVER_CURR</seealso>
    /// </summary>
    public int PluginApiVersion;

    /// <summary>
    /// Silent mode flag
    /// </summary>
    public int Silent;

    /// <summary>
    /// Install directory error flag
    /// </summary>
    public int InstdirError;

    /// <summary>
    /// Indicates whether the language is right-to-left (RTL); 1 means RTL
    /// </summary>
    public int Rtl;

    /// <summary>
    /// Error level or error code.
    /// </summary>
    public int Errlvl;

    /// <summary>
    /// Specifies the registry view. Default view = 0; the alternative view depends on pointer size:
    /// on 64-bit processes (sizeof(void*) &gt; 4) the alternate view is typically KEY_WOW64_32KEY,
    /// otherwise KEY_WOW64_64KEY. The field stores the corresponding Windows registry view flag as an integer
    /// </summary>
    public int AlterRegView;

    /// <summary>
    /// Enable status update / details printing
    /// </summary>
    public int StatusUpdate;
}
