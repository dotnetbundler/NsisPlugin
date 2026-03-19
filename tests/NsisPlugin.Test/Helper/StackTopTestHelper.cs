namespace NsisPlugin.Test.Helper;

public static unsafe class StackTopTestHelper
{
    public static IntPtr Create() => TestUnmanagedMemory.Zeroed<IntPtr>();

    public static void DrainAndFree(StackT? stack)
    {
        if (stack is null) return;

        while (stack.Pop(out _)) { }

        TestUnmanagedMemory.Free((IntPtr)stack.Raw);
    }
}
