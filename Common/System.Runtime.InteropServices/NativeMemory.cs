#if !NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace System.Runtime.InteropServices;

internal static unsafe class NativeMemory
{
    public static void* Alloc(UIntPtr byteCount) => (void*)Marshal.AllocHGlobal((nint)(void*)byteCount);

    public static void* Alloc(UIntPtr elementCount, UIntPtr elementSize) => Alloc((nuint)elementCount * elementSize);

    public static void* AllocZeroed(UIntPtr byteCount)
    {
        var ptr = Alloc(byteCount);
        Clear(ptr, byteCount);
        return ptr;
    }

    public static void* AllocZeroed(UIntPtr elementCount, UIntPtr elementSize) => AllocZeroed((nuint)elementCount * elementSize);

    public static void* Realloc(void* ptr, UIntPtr byteCount) => (void*)Marshal.ReAllocHGlobal((nint)ptr, (nint)(void*)byteCount);

    public static void Free(void* ptr)
    {
        if (ptr != null)
        {
            Marshal.FreeHGlobal((nint)ptr);
        }
    }

    public static void Clear(void* ptr, UIntPtr byteCount) => Fill(ptr, byteCount, 0);

    public static void Copy(void* source, void* destination, UIntPtr byteCount) => Buffer.MemoryCopy(source, destination, (long)byteCount, (long)byteCount);

    [SuppressMessage("ReSharper", "ShiftExpressionRealShiftCountIsZero")]
    public static void Fill(void* ptr, UIntPtr byteCount, byte value)
    {
        var p = (byte*)ptr;
        nuint length = byteCount;

        // 构建原生字长填充模式：将单字节值扩展到 4 字节（32位）或 8 字节（64位）
        nuint fillValue = value;
        fillValue |= fillValue << 8;
        fillValue |= fillValue << 16;
        if (sizeof(nuint) == 8) fillValue |= fillValue << 32;

        // 按原生字长4 字节或 8 字节步进填充
        var stepSize = (nuint)sizeof(nuint);
        while (length >= stepSize)
        {
            *(nuint*)p = fillValue;
            p += stepSize;
            length -= stepSize;
        }

        // 剩余的零碎字节（最多 7 个）
        while (length > 0)
        {
            *p = value;
            p++;
            length--;
        }
    }

    public static void* AlignedAlloc(UIntPtr byteCount, UIntPtr alignment)
    {
        if (alignment == UIntPtr.Zero) throw new ArgumentException("Alignment cannot be zero.", nameof(alignment));

        nuint align = alignment;
        if ((align & (align - 1)) != 0) throw new ArgumentException("Alignment must be a power of two.", nameof(alignment));

        var extra = align + (nuint)sizeof(void*);
        var totalBytes = byteCount + extra;

        var originalPtr = Alloc(totalBytes);

        var alignedPtr = ((nuint)originalPtr + (nuint)sizeof(void*) + align - 1) & ~(align - 1);

        var header = (void**)alignedPtr - 1;
        *header = originalPtr;

        return (void*)alignedPtr;
    }

    public static void AlignedFree(void* ptr)
    {
        if (ptr == null) return;

        var header = (void**)ptr - 1;
        var originalPtr = *header;

        Free(originalPtr);
    }

    public static void* AlignedRealloc(void* ptr, UIntPtr byteCount, UIntPtr alignment) => throw new NotSupportedException("AlignedRealloc is not supported in this implementation.");
}
#endif
