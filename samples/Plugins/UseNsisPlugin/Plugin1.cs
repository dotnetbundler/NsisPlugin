using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin1
{
    [NsisAction]
    public static int Add(int a, int b) => a + b;

    [NsisAction]
    public static string StrAdd(string a, string b) => a + b;
}
