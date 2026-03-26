using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin
{
    [NsisAction(Encoding = NsEncoding.Unicode)]
    public static int Add(int a, int b) => a + b;
}
