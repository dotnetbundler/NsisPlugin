using System.Runtime.InteropServices;

namespace NsisPlugin;

/// <summary>
/// NSIS插件回调委托
/// 当插件或图形界面被卸载时调用
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L37">NSISPLUGINCALLBACK Source</seealso>
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr NsPluginCallback(Nspim message);
