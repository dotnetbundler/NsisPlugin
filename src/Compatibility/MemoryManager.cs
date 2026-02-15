using System.Runtime.InteropServices;

namespace NsisPlugin.Compatibility;

/// <summary>
/// 跨框架内存管理 API 统一接口
/// 为不同 .NET 版本提供统一的内存分配和释放方法
/// </summary>
public static unsafe class MemoryManager
{
    /// <summary>
    /// 分配零初始化的非托管内存
    /// </summary>
    /// <param name="byteCount">要分配的字节数</param>
    /// <returns>指向分配内存的指针</returns>
    public static void* AllocZeroed(nuint byteCount)
    {
#if NET6_0_OR_GREATER
        return NativeMemory.AllocZeroed(byteCount);
#else
        var ptr = Marshal.AllocHGlobal((int)byteCount);
        var p = (byte*)ptr;
        for (nuint i = 0; i < byteCount; i++) p[i] = 0;
        return (void*)ptr;
#endif
    }

    /// <summary>
    /// 释放非托管内存
    /// </summary>
    /// <param name="ptr">指向要释放的内存的指针</param>
    public static void Free(void* ptr)
    {
#if NET6_0_OR_GREATER
        NativeMemory.Free(ptr);
#else
        Marshal.FreeHGlobal((IntPtr)ptr);
#endif
    }
}
