using System.Runtime.InteropServices;

namespace NsisPlugin.Test.Helper;

public static class TestUnmanagedMemory
{
    /// <summary>
    /// 分配并清零非托管内存，统一测试中的 Zeroed 语义。
    /// </summary>
    public static IntPtr Zeroed(int byteCount)
    {
        unsafe { return (IntPtr)NativeMemory.AllocZeroed((nuint)byteCount); }
    }

    public static IntPtr Zeroed(nuint byteCount)
    {
        unsafe { return (IntPtr)NativeMemory.AllocZeroed(byteCount); }
    }

    public static IntPtr Zeroed<T>() where T : unmanaged
    {
        unsafe { return (IntPtr)NativeMemory.AllocZeroed((nuint)sizeof(T)); }
    }

    public static void Free(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return;
        unsafe { NativeMemory.Free((void*)ptr); }
    }
}
