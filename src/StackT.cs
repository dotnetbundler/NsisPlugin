using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NsisPlugin.NsisApi;

namespace NsisPlugin;

public unsafe class StackT(IntPtr stackTop)
{
    public stack_t** Raw { get; } = (stack_t**)stackTop;

    public bool PopString([NotNullWhen(true)] out string? str)
    {
        if (Raw is null || *Raw is null)
        {
            str = null;
            return false;
        }

        var stackNode = *Raw;
        str = PluginEncoding.PtrToString((IntPtr)(&stackNode->text))!;
        *Raw = stackNode->next;
        NativeMemory.Free(stackNode);
        return true;
    }

    public bool PushString(string str)
    {
        if (Raw is null) return false;

        var bytes = PluginEncoding.Encoding.GetBytes(str);
        var stackNode = (stack_t*)NativeMemory.AllocZeroed((nuint)(sizeof(IntPtr) + PluginApi.MaxStringBytes));
        Marshal.Copy(bytes, 0, (IntPtr)(&stackNode->text), Math.Min(bytes.Length, PluginApi.MaxStringBytes - PluginEncoding.CharSize));
        stackNode->next = *Raw;
        *Raw = stackNode;
        return true;
    }
}
