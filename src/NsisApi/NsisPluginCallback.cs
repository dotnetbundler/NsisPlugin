using System.Runtime.InteropServices;

namespace NsisPlugin.NsisApi;

/// <summary>
/// NSIS Plugin Callback Delegate
/// It is called when the plugin or GUI is uninstalled.
///
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L37">NSISPLUGINCALLBACK Source</seealso>
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr NsisPluginCallback(Nspim message);
