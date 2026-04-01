using System.Runtime.InteropServices;
using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin6
{
    /// <summary>
    /// 注册回调函数
    /// </summary>
    [NsisAction]
    public static int RegisterCallback(bool isShowMessageBox)
    {
        return NsPlugin.ExtraParameters.RegisterPluginCallback((nspim) =>
        {
            if (nspim == Nspim.NspimGuiunload)
            {
                // 在这里执行卸载界面时的回调逻辑
                if (isShowMessageBox) MessageBox(IntPtr.Zero, "回调已触发: GUI is unloading", "插件调试", 0);
            }
            else
            {
                // 在这里执行卸载时的回调逻辑
                if (isShowMessageBox) MessageBox(IntPtr.Zero, "回调已触发: Plugin is unloading", "插件调试", 0);
            }
            return nint.Zero;
        });

        [DllImport("user32.dll")]
        static extern int MessageBox(IntPtr hWnd, string lpText, string lpCaption, uint uType);
    }
}
