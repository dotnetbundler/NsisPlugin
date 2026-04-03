using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NsisPlugin.NsisApi;

namespace NsisPlugin;

/// <summary>
/// NSIS 插件栈封装
/// </summary>
/// <param name="stackTop">栈顶二级指针</param>
public unsafe class StackT(IntPtr stackTop)
{
    /// <summary>
    /// 原始栈顶二级指针
    /// </summary>
    public stack_t** Raw { get; } = (stack_t**)stackTop;

    /// <summary>
    /// 从 NSIS 插件栈顶弹出一个字符串
    /// </summary>
    /// <param name="str">弹出的值</param>
    /// <returns>是否成功</returns>
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
        NativeMemory.Free(stackNode);
        return true;
    }

    /// <summary>
    /// 向 NSIS 插件栈顶压入一个字符串
    /// </summary>
    /// <param name="str">推送值</param>
    /// <returns>是否成功</returns>
    public bool Push(string str)
    {
        if (Raw is null) return false;

        var stackNode = (stack_t*)NativeMemory.AllocZeroed((nuint)(sizeof(IntPtr) + NsPlugin.MaxStringBytes));
        NsPluginEnc.CopyStringToBuffer(str, (IntPtr)(&stackNode->text), NsPlugin.MaxStringBytes);
        stackNode->next = *Raw;
        *Raw = stackNode;
        return true;
    }
}
