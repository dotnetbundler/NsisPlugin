using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NsisPlugin.Compatibility;
using NsisPlugin.NsisApi;

namespace NsisPlugin;

public unsafe class StackT(IntPtr stackTop)
{
    public stack_t** Raw { get; } = (stack_t**)stackTop;

    public bool Pop([NotNullWhen(true)] out string? str)
    {
        if (Raw is null || *Raw is null)
        {
            str = null;
            return false;
        }

        var stackNode = *Raw;
        str = NsPluginEnc.PtrToString((IntPtr)(&stackNode->text))!;
        *Raw = stackNode->next;
        MemoryManager.Free(stackNode);
        return true;
    }

    public bool Push(string str)
    {
        if (Raw is null) return false;

        var bytes = NsPluginEnc.Encoding.GetBytes(str);
        var stackNode = (stack_t*)MemoryManager.AllocZeroed((nuint)(sizeof(IntPtr) + NsPlugin.MaxStringBytes));
        Marshal.Copy(bytes, 0, (IntPtr)(&stackNode->text), Math.Min(bytes.Length, NsPlugin.MaxStringBytes - NsPluginEnc.CharSize));
        stackNode->next = *Raw;
        *Raw = stackNode;
        return true;
    }
}
