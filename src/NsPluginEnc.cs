using System.Runtime.InteropServices;
using System.Text;

namespace NsisPlugin;

/// <summary>
/// 插件编码
/// </summary>
public static class NsPluginEnc
{
    /// <summary>
    /// 全局编码设置，默认为 false（ANSI）
    /// 如果设置为 true，则所有线程默认使用 Unicode 编码，除非线程本地设置覆盖它
    /// </summary>
    public static bool IsGlobalUnicode { get; set; }

    /// <summary>
    /// 范围编码设置，默认为 null，表示使用全局设置
    /// 如果设置为 true，则该范围使用 Unicode 编码；如果设置为 false，则该线程使用 ANSI 编码
    /// </summary>
    [field: ThreadStatic] public static bool? IsScopeUnicode { get; set; }

    /// <summary>
    /// 获取是否使用 Unicode 编码
    /// </summary>
    public static bool IsUnicode => IsScopeUnicode ?? IsGlobalUnicode;

    /// <summary>
    /// 获取每个字符的字节数，取决于当前编码
    /// </summary>
    public static int CharSize => IsUnicode ? 2 : 1;

    /// <summary>
    /// 获取当前编码对应的 Encoding 对象
    /// </summary>
    public static Encoding Encoding => IsUnicode ? Encoding.Unicode : Encoding.Default;

    /// <summary>
    /// 将指针指向的字符串转换为 C# 字符串，取决于当前编码
    /// </summary>
    /// <param name="ptr">字符串指针</param>
    /// <returns>字符串</returns>
    public static string? PtrToString(IntPtr ptr) => IsUnicode ? Marshal.PtrToStringUni(ptr) : Marshal.PtrToStringAnsi(ptr);

    /// <summary>
    /// 创建编码作用域，在 using 块内临时切换编码设置，离开块后恢复之前的设置
    /// </summary>
    /// <param name="isUnicode">是否使用 Unicode 编码，如果为 null 则使用全局设置</param>
    /// <returns>编码作用域对象</returns>
    public static IDisposable CreateEncScope(bool? isUnicode) => new NsPluginEncScope(isUnicode);
}

/// <summary>
/// 编码作用域管理类
/// 用于在 code block 范围内临时切换编码设置
/// </summary>
public sealed class NsPluginEncScope : IDisposable
{
    private readonly bool? _pre = NsPluginEnc.IsScopeUnicode;
    public NsPluginEncScope(bool? isUnicode) => NsPluginEnc.IsScopeUnicode = isUnicode;
    public void Dispose() => NsPluginEnc.IsScopeUnicode = _pre;
}
