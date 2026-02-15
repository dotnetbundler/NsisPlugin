using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace NsisPlugin;

public class Variables(IntPtr variables)
{
    public IntPtr Raw => variables;

    public bool Get(NsVariable variable, [NotNullWhen(true)] out string? value)
    {
        if (variables == IntPtr.Zero || variable is < NsVariable.Inst0 or >= NsVariable.InstLast)
        {
            value = null;
            return false;
        }

        var variablePtr = variables + ((int)variable * NsPlugin.MaxStringBytes);
        value = PluginEncoding.PtrToString(variablePtr)!;
        return true;
    }

    public bool Set(NsVariable variable, string value)
    {
        if (variables == IntPtr.Zero || variable is < NsVariable.Inst0 or >= NsVariable.InstLast) return false;

        var bytes = PluginEncoding.Encoding.GetBytes(value);
        var variablePtr = variables + ((int)variable * NsPlugin.MaxStringBytes);
        Marshal.Copy(bytes, 0, variablePtr, Math.Min(bytes.Length, NsPlugin.MaxStringBytes - PluginEncoding.CharSize));
        return true;
    }
}
