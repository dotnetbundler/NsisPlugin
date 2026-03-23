#pragma warning disable CS8618

namespace NsisPlugin;

/// <summary>
/// NSIS 插件全局上下文
/// </summary>
public static class NsPlugin
{
    /// <summary>
    /// 插件模块句柄
    /// </summary>
    public static IntPtr ModuleHandle { get; set; }

    /// <summary>
    /// 父窗口句柄
    /// </summary>
    [field: ThreadStatic] public static IntPtr HwndParent { get; private set; }

    /// <summary>
    /// 字符串缓冲区大小（以字符为单位）
    /// </summary>
    [field: ThreadStatic] public static int StringSize { get; private set; }

    /// <summary>
    /// NSIS 插件变量封装
    /// </summary>
    [field: ThreadStatic] public static Variables Variables { get; private set; }

    /// <summary>
    /// NSIS 插件栈封装
    /// </summary>
    [field: ThreadStatic] public static StackT StackTop { get; private set; }

    /// <summary>
    /// NSIS 插件额外参数封装
    /// </summary>
    [field: ThreadStatic] public static ExtraParameters ExtraParameters { get; private set; }

    /// <summary>
    /// 字符串缓冲区大小（以字节为单位）
    /// </summary>
    public static int MaxStringBytes => StringSize * NsPluginEnc.CharSize;

    /// <summary>
    /// 初始化 NSIS 插件上下文
    /// </summary>
    /// <param name="hwndParent">父窗口句柄</param>
    /// <param name="stringSize">字符串缓冲区大小</param>
    /// <param name="variables">nsis 变量指针</param>
    /// <param name="stacktop">nsis 插件栈二级指针</param>
    /// <param name="extra">nsis 插件额外参数指针</param>
    public static void Init(IntPtr hwndParent, int stringSize, IntPtr variables, IntPtr stacktop, IntPtr extra)
    {
        HwndParent = hwndParent;
        StringSize = stringSize;
        Variables = new Variables(variables);
        StackTop = new StackT(stacktop);
        ExtraParameters = new ExtraParameters(extra);
    }
}
