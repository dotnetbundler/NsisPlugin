#pragma warning disable CS8618

namespace NsisPlugin;

public static class NsPlugin
{
    public static IntPtr ModuleHandle { get; set; }

    [field: ThreadStatic] public static IntPtr HwndParent { get; private set; }

    [field: ThreadStatic] public static int StringSize { get; private set; }

    [field: ThreadStatic] public static Variables Variables { get; private set; }

    [field: ThreadStatic] public static StackT StackTop { get; private set; }

    [field: ThreadStatic] public static ExtraParameters ExtraParameters { get; private set; }

    public static int MaxStringBytes => StringSize * PluginEncoding.CharSize;

    public static void Init(IntPtr hwndParent, int stringSize, IntPtr variables, IntPtr stacktop, IntPtr extraParameters)
    {
        HwndParent = hwndParent;
        StringSize = stringSize;
        Variables = new Variables(variables);
        StackTop = new StackT(stacktop);
        ExtraParameters = new ExtraParameters(extraParameters);
    }
}
