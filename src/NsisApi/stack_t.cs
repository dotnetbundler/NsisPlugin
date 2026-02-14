using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace NsisPlugin.NsisApi;

/// <summary>
/// 字符串栈的链表节点
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Contrib/ExDLL/pluginapi.h#L20-L27">stack_t Source</seealso>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct stack_t
{
    /// <summary>
    /// 指向下一个堆栈节点的指针（或 null）
    /// </summary>
    public stack_t* next;

    /// <summary>
    /// 内联字符串的第一个字节
    /// 该字段之后的字符串字节在内存中依次跟随，并通常以空字节结尾
    /// 不要将此字段视为独立的字节
    /// 要读取完整的字符串，请从该地址开始读取字节，直到找到空字节终止符
    /// </summary>
    public byte text;
}
