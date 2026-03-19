using NsisPlugin.Compatibility;

namespace NsisPlugin.Test.Helper;

public static unsafe class StackTopTestHelper
{
    public static IntPtr Create() => (IntPtr)MemoryManager.AllocZeroed((nuint)sizeof(IntPtr));

    public static void DrainAndFree(StackT? stack)
    {
        if (stack is null) return;

        while (stack.Pop(out _)) { }

        MemoryManager.Free(stack.Raw);
    }
}
