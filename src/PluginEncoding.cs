using System.Runtime.InteropServices;
using System.Text;

namespace NsisPlugin;

/// <summary>
/// 插件编码
/// </summary>
public static class PluginEncoding
{
    /// <summary>
    /// 全局编码设置，默认为 false（ANSI）
    /// 如果设置为 true，则所有线程默认使用 Unicode 编码，除非线程本地设置覆盖它
    /// </summary>
    public static bool IsGlobalUnicode { get; set; }

    /// <summary>
    /// 线程本地编码设置，默认为 null，表示使用全局设置
    /// 如果设置为 true，则该线程使用 Unicode 编码；如果设置为 false，则该线程使用 ANSI 编码
    /// </summary>
    [field: ThreadStatic] public static bool? IsThreadUnicode { get; set; }

    /// <summary>
    /// 获取是否使用 Unicode 编码
    /// </summary>
    public static bool IsUnicode => IsThreadUnicode ?? IsGlobalUnicode;

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
}
