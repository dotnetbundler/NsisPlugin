using System.Reflection;
using System.Runtime.InteropServices;

namespace NsisPlugin;

public static class PluginApi
{
    public static IntPtr ModuleHandle { get; private set; }

    [field: ThreadStatic] public static IntPtr HwndParent { get; private set; }

    [field: ThreadStatic] public static int StringSize { get; private set; }

    [field: ThreadStatic] public static Variables Variables { get; private set; }

    [field: ThreadStatic] public static StackT StackTop { get; private set; }

    [field: ThreadStatic] public static ExtraParameters ExtraParameters { get; private set; }

    public static int MaxStringBytes => StringSize * PluginEncoding.CharSize;

    static PluginApi()
    {
        Variables = null!;
        StackTop = null!;
        ExtraParameters = null!;
        ModuleHandle = GetModuleHandle($"{Assembly.GetExecutingAssembly().GetName().Name}.dll");

        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public static void Init(IntPtr hwndParent, int stringSize, IntPtr variables, IntPtr stacktop, IntPtr extraParameters)
    {
        HwndParent = hwndParent;
        StringSize = stringSize;
        Variables = new Variables(variables);
        StackTop = new StackT(stacktop);
        ExtraParameters = new ExtraParameters(extraParameters);
    }
}
