using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NsisPlugin.NsisApi;

namespace NsisPlugin;

public unsafe class StackT(stack_t** stackTop)
{
    public stack_t** Raw => stackTop;

    public bool PopString([NotNullWhen(true)] out string? str)
    {
        if (stackTop is null || *stackTop is null)
        {
            str = null;
            return false;
        }

        var stackNode = *stackTop;
        str = PluginEncoding.PtrToString(stackNode->text)!;
        *stackTop = stackNode->next;
        NativeMemory.Free(stackNode);
        return true;
    }

    public bool PushString(string str)
    {
        if (stackTop is null) return false;

        var bytes = PluginEncoding.Encoding.GetBytes(str);
        var stackNode = (stack_t*)NativeMemory.AllocZeroed((nuint)(sizeof(IntPtr) + PluginApi.MaxStringBytes));
        Marshal.Copy(bytes, 0, (IntPtr)(&stackNode->text), Math.Min(bytes.Length, PluginApi.MaxStringBytes - PluginEncoding.CharSize));
        stackNode->next = *stackTop;
        *stackTop = stackNode;
        return true;
    }
}
