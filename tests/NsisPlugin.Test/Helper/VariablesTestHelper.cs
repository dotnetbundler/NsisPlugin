using NsisPlugin.Compatibility;

namespace NsisPlugin.Test.Helper;

public static unsafe class VariablesTestHelper
{
    public static IntPtr Create(NsEncoding encoding, int stringSize)
    {
        if (encoding == NsEncoding.Undefined) throw new NotSupportedException("Undefined encoding");

        var charSize = encoding == NsEncoding.Unicode ? 2 : 1;
        var byteCount = (nuint)(stringSize * charSize * (int)NsVariable.InstLast);
        return (IntPtr)MemoryManager.AllocZeroed(byteCount);
    }

    public static void Free(IntPtr variablesPtr) => MemoryManager.Free((void*)variablesPtr);
}
