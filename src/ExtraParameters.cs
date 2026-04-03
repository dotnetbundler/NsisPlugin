using System.Runtime.InteropServices;
using NsisPlugin.NsisApi;

namespace NsisPlugin;

/// <summary>
/// NSIS 插件额外参数封装
/// </summary>
/// <param name="extraPtr">额外参数指针</param>
public unsafe class ExtraParameters(IntPtr extraPtr)
{
    private static NsPluginCallback? _callback;
    private static IntPtr _callbackPtr;

    /// <summary>
    /// 原始额外参数指针
    /// </summary>
    public extra_parameters* Raw { get; } = (extra_parameters*)extraPtr;
    /// <summary>
    /// NSIS 插件执行标志
    /// </summary>
    public ref ExecFlags ExecFlags => ref *Raw->exec_flags;

    /// <summary>
    /// 执行 NSIS 代码段
    /// </summary>
    /// <param name="code">要执行的 NSIS 代码段编号</param>
    /// <returns>成功返回 0</returns>
    public int ExecuteCodeSegment(int code) => Raw->ExecuteCodeSegment(code, NsPlugin.HwndParent);

    /// <summary>
    /// 验证并规范化文件名参数
    /// </summary>
    /// <param name="filename">文件名</param>
    public void ValidateFilename(ref string filename)
    {
        var buffer = NativeMemory.Alloc((nuint)NsPlugin.MaxStringBytes);
        try
        {
            var bufferPtr = (IntPtr)buffer;
            NsPluginEnc.CopyStringToBuffer(filename, bufferPtr, NsPlugin.MaxStringBytes);
            Raw->validate_filename(bufferPtr);
            filename = NsPluginEnc.PtrToString(bufferPtr)!;
        }
        finally
        {
            NativeMemory.Free(buffer);
        }
    }

    /// <summary>
    /// 注册插件回调函数
    /// </summary>
    /// <param name="callback">回调函数</param>
    /// <returns>成功返回 0，已经注册返回 1</returns>
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
