using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin4
{
    /// <summary>
    /// 自动关闭标志位
    /// </summary>
    /// <param name="extra"></param>
    /// <param name="value">是否自动关闭1/0</param>
    [NsisAction]
    public static void AutoClose(ExtraParameters extra, int value)
    {
        extra.ExecFlags.Autoclose = value;
    }
}
