using System.Runtime.InteropServices;

namespace Hello;

internal class Hello
{
    [UnmanagedCallersOnly(EntryPoint = "SayHi")]
    public static void SayHi()
    {
        Console.WriteLine("Hello, World!");
    }
}
