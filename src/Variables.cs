using System.Diagnostics.CodeAnalysis;

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

        var variablePtr = variables + ((int)variable * NsPlugin.MaxStringBytes);
        NsPluginEnc.CopyStringToBuffer(value, variablePtr, NsPlugin.MaxStringBytes);
        return true;
    }
}
