using System.Runtime.InteropServices;

namespace NsisPlugin;

/// <summary>
/// NSIS 执行标志
/// int 当用作布尔标志时，它们遵循0表示false，非0表示true的约定
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L39-L57">exec_flags_t Source</seealso>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct ExecFlags
{
    /// <summary>
    /// 自动关闭标志
    /// </summary>
    public int Autoclose;

    /// <summary>
    /// 变量作用域
    /// 用户上下文 = 0，机器上下文 = 1
    /// </summary>
    public int AllUserVar;

    /// <summary>
    /// 执行错误标志
    /// </summary>
    public int ExecError;

    /// <summary>
    /// 中止标志
    /// </summary>
    public int Abort;

    /// <summary>
    /// 指示是否需要重启
    /// </summary>
    public int ExecReboot;

    /// <summary>
    /// 指示是否已调用重启
    /// </summary>
    public int RebootCalled;

    /// <summary>
    /// 已弃用；保留以支持向后兼容的ABI/布局
    /// </summary>
    public int XxxCurInsttype;

    /// <summary>
    /// 插件API/ABI版本
    /// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L24-L25">NSISPIAPIVER_CURR</seealso>
    /// </summary>
    public int PluginApiVersion;

    /// <summary>
    /// 静默模式标志
    /// </summary>
    public int Silent;

    /// <summary>
    /// 安装目录错误标志
    /// </summary>
    public int InstdirError;

    /// <summary>
    /// 表示语言是否为从右到左（RTL）；1 表示 RTL
    /// </summary>
    public int Rtl;

    /// <summary>
    /// 错误级别或错误代码
    /// </summary>
    public int Errlvl;

    /// <summary>
    /// 指定注册表视图
    /// </summary>
    public int AlterRegView;

    /// <summary>
    /// 启用状态更新 / 详细信息打印
    /// </summary>
    public int StatusUpdate;
}
