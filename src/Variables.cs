using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NsisPlugin;

/// <summary>
/// NSIS 插件变量封装
/// </summary>
/// <param name="variables"></param>
public class Variables(IntPtr variables)
{
    /// <summary>
    /// 原始变量指针
    /// </summary>
    public IntPtr Raw => variables;

    /// <summary>
    /// 获取 NSIS 变量值
    /// </summary>
    /// <param name="variable">变量</param>
    /// <param name="value">获取值</param>
    /// <returns>是否成功</returns>
    public bool Get(NsVariable variable, [NotNullWhen(true)] out string? value)
    {
        if (variables == IntPtr.Zero || variable is < NsVariable.Inst0 or >= NsVariable.InstLast)
        {
            value = null;
            return false;
        }

        var variablePtr = variables + ((int)variable * NsPlugin.MaxStringBytes);
        value = NsPluginEnc.PtrToString(variablePtr)!;
        return true;
    }

    /// <summary>
    /// 设置 NSIS 变量值
    /// </summary>
    /// <param name="variable">变量</param>
    /// <param name="value">设置值</param>
    /// <returns>是否成功</returns>
    public bool Set(NsVariable variable, string value)
    {
        if (variables == IntPtr.Zero || variable is < NsVariable.Inst0 or >= NsVariable.InstLast) return false;

        var bytes = NsPluginEnc.Encoding.GetBytes(value);
        // 拷贝
        var variablePtr = variables + ((int)variable * NsPlugin.MaxStringBytes);
        var copyLength = Math.Min(bytes.Length, NsPlugin.MaxStringBytes - NsPluginEnc.CharSize);
        Marshal.Copy(bytes, 0, variablePtr, copyLength);
        // 确保字符串以 null 结尾
        for (var i = 0; i < NsPluginEnc.CharSize; i++) Marshal.WriteByte(variablePtr, copyLength + i, 0);
        return true;
    }
}
