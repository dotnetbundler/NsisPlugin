using System.Runtime.InteropServices;

// ReSharper disable once InconsistentNaming

namespace NsisPlugin.NsisApi;

/// <summary>
/// 用于与原生NSIS/插件主机进行互操作
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L66-L71">extra_parameters Source</seealso>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct extra_parameters
{
    /// <summary>
    /// 指向 ExecFlags 的指针
    /// </summary>
    public ExecFlags* exec_flags;

    /// <summary>
    /// 执行代码段
    /// 成功返回 0
    /// </summary>
    public delegate* unmanaged[Stdcall]<int, IntPtr, int> ExecuteCodeSegment;

    /// <summary>
    /// 验证文件名
    /// </summary>
    public delegate* unmanaged[Stdcall]<IntPtr, void> validate_filename;

    /// <summary>
    /// 注册插件回调
    /// 成功返回 0，已注册返回 1，错误返回 &lt;0
    /// </summary>
    public delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int> RegisterPluginCallback;
}
