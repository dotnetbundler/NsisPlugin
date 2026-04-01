using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin6
{
    /// <summary>
    /// 注册回调函数
    /// </summary>
    [NsisAction]
    public static int RegisterCallback()
    {
        return NsPlugin.ExtraParameters.RegisterPluginCallback((nspim) =>
        {
            if (nspim == Nspim.NspimGuiunload)
            {
                // 在这里执行卸载界面时的回调逻辑
            }
            else
            {
                // 在这里执行卸载时的回调逻辑
            }
            return nint.Zero;
        });
    }
}
