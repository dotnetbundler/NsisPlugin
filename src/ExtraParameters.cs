using System.Runtime.InteropServices;
using NsisPlugin.Compatibility;
using NsisPlugin.NsisApi;

namespace NsisPlugin;

public unsafe class ExtraParameters(IntPtr extraParameters)
{
    private static NsPluginCallback? _callback;
    private static IntPtr _callbackPtr;

    public extra_parameters* Raw { get; } = (extra_parameters*)extraParameters;
    public ref ExecFlags ExecFlags => ref *Raw->exec_flags;

    public int ExecuteCodeSegment(int code) => Raw->ExecuteCodeSegment(code, NsPlugin.HwndParent);

    public void ValidateFilename(ref string filename)
    {
        var buffer = MemoryManager.AllocZeroed((nuint)NsPlugin.MaxStringBytes);
        try
        {
            var bufferPtr = (IntPtr)buffer;
            var bytes = NsPluginEnc.Encoding.GetBytes(filename);
            Marshal.Copy(bytes, 0, bufferPtr, Math.Min(bytes.Length, NsPlugin.MaxStringBytes - NsPluginEnc.CharSize));
            Raw->validate_filename(bufferPtr);
            filename = NsPluginEnc.PtrToString(bufferPtr)!;
        }
        finally
        {
            MemoryManager.Free(buffer);
        }
    }

    public int RegisterPluginCallback(NsPluginCallback callback)
    {
        // 如果已经注册过回调函数了，就直接返回 1，表示已经注册过了
        if (_callbackPtr != IntPtr.Zero) return 1;

        // 注册回调函数
        var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        var res = Raw->RegisterPluginCallback(NsPlugin.ModuleHandle, callbackPtr);
        if (res != 0) return res;

        // 成功记录 _callback 保持引用以防止垃圾回收
        _callback = callback;
        _callbackPtr = callbackPtr;
        return res;
    }
}
