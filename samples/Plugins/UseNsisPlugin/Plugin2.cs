using System.Runtime.InteropServices;
using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin2
{
    /// <summary>
    /// 移动窗口
    /// </summary>
    /// <param name="location">
    /// 窗口位置参数
    /// 格式为"XY"，X表示水平位置，Y表示垂直位置。
    ///     X: L（左），C（居中），R（右）
    ///     Y: T（上），C（居中），B（下）
    /// </param>
    [NsisAction]
    public static void MoveWindow(string location)
    {
        if (location.Length != 2) return;
        if (!GetWindowRect(NsPlugin.HwndParent, out var rect)) return;
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        var screenWidth = GetSystemMetrics(0);
        var screenHeight = GetSystemMetrics(1);

        var x = char.ToUpper(location[0]) switch
        {
            'L' => 0,
            'C' => (screenWidth - width) / 2,
            'R' => screenWidth - width,
            _ => 0
        };
        var y = char.ToUpper(location[1]) switch
        {
            'T' => 0,
            'C' => (screenHeight - height) / 2,
            'B' => screenHeight - height,
            _ => 0
        };

        // 修改窗口位置,uFlags: 0x0001: 不改变窗口大小, 0x0004: 不改变Z序
        SetWindowPos(NsPlugin.HwndParent, IntPtr.Zero, x, y, 0, 0, 0x0001 | 0x0004);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
