using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NotUseNsisPlugin;

public unsafe class Plugin
{
    [UnmanagedCallersOnly(EntryPoint = "Add", CallConvs = [typeof(CallConvCdecl)])]
    public static void Add(IntPtr hwndParent, int string_size, IntPtr variables, IntPtr stacktop, IntPtr extra)
    {
        var a = PopString(stacktop, string_size);
        var b = PopString(stacktop, string_size);

        if (int.TryParse(a, out var aValue) && int.TryParse(b, out var bValue))
        {
            var result = aValue + bValue + 1;
            PushString(stacktop, string_size, result.ToString());
            return;
        }

        PushString(stacktop, string_size, "Error: Invalid input");
    }

    /// <summary>
    /// 从栈顶弹出一个字符串，并返回其值。字符串的最大长度由 stringSize 参数指定。
    /// 这里假设是 Unicode 编码
    /// </summary>
    /// <param name="pStackTop"></param>
    /// <param name="stringSize"></param>
    /// <returns></returns>
    private static string PopString(IntPtr pStackTop, int stringSize)
    {
        // 获取当前栈顶节点的地址
        var pCurrent = Marshal.ReadIntPtr(pStackTop);
        if (pCurrent == IntPtr.Zero) return string.Empty;

        // 获取数据部分的地址（跳过 next 指针）
        var pData = pCurrent + IntPtr.Size;
        var value = Marshal.PtrToStringUni(pData);

        // 将栈顶指针指向下一个节点 (Pop 操作)
        var pNext = Marshal.ReadIntPtr(pCurrent);
        Marshal.WriteIntPtr(pStackTop, pNext);

        // 释放当前节点的内存
        NativeMemory.Free((void*)pCurrent);

        return value ?? string.Empty;
    }

    /// <summary>
    /// 将一个字符串压入栈顶。字符串的最大长度由 stringSize 参数指定。
    /// 这里假设是 Unicode 编码
    /// </summary>
    /// <param name="pStackTop"></param>
    /// <param name="stringSize"></param>
    /// <param name="value"></param>
    private static void PushString(IntPtr pStackTop, int stringSize, string value)
    {
        // 1. 计算需要的总内存：指针大小 + 字符串缓冲区大小
        var blockSize = IntPtr.Size + (stringSize * 2);

        var pNew = (IntPtr)NativeMemory.AllocZeroed((nuint)blockSize);

        // 2. 获取当前的栈顶地址，存入新节点的 next
        var pOldTop = Marshal.ReadIntPtr(pStackTop);
        Marshal.WriteIntPtr(pNew, pOldTop);

        // 3. 写入字符串到数据区
        var bytes = System.Text.Encoding.Unicode.GetBytes(value);
        Marshal.Copy(bytes, 0, pNew + IntPtr.Size, Math.Min(bytes.Length, (stringSize * 2) - 2));

        // 4. 更新栈顶指针为新节点
        Marshal.WriteIntPtr(pStackTop, pNew);
    }
}
