using System.Runtime.InteropServices;
using NsisPlugin.NsisApi;

namespace NsisPlugin;

public unsafe class ExtraParameters(IntPtr extraParameters)
{
    // 这里记录回调函数的委托实例和函数指针
    // 防止它们被垃圾回收掉导致回调失效
    private static NsisPluginCallback? _callback;
    private static IntPtr _callbackPtr;

    public extra_parameters* Raw { get; } = (extra_parameters*)extraParameters;

    public ExecFlags ExecFlags { get => *Raw->exec_flags; set => *Raw->exec_flags = value; }

    public int ExecuteCodeSegment(int code) => Raw->ExecuteCodeSegment(code, PluginApi.HwndParent);

    public void ValidateFilename(ref string filename)
    {
        var buffer = (IntPtr)NativeMemory.AllocZeroed((nuint)PluginApi.MaxStringBytes);
        try
        {
            var bytes = PluginEncoding.Encoding.GetBytes(filename);
            Marshal.Copy(bytes, 0, buffer, Math.Min(bytes.Length, PluginApi.MaxStringBytes - PluginEncoding.CharSize));
            Raw->validate_filename(buffer);
            filename = PluginEncoding.PtrToString(buffer)!;
        }
        finally
        {
            NativeMemory.Free((void*)buffer);
        }
    }

    public int RegisterPluginCallback(NsisPluginCallback callback)
    {
        // 如果已经注册过回调函数了，就直接返回 1，表示已经注册过了
        if (_callbackPtr != IntPtr.Zero) return 1;

        // 注册回调函数
        var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        var res = Raw->RegisterPluginCallback(PluginApi.ModuleHandle, callbackPtr);
        if (res != 0) return res;

        // 成功记录
        _callback = callback;
        _callbackPtr = callbackPtr;
        return res;
    }
}
