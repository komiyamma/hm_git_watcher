using System;
using System.Runtime.InteropServices;

namespace HmGitWatcher;

public partial class HmGitWatcher
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetDpiForWindow(IntPtr hWnd);

    public int GetDpiFromWindowHandle(long hWnd)
    {
        if ((IntPtr)hWnd == IntPtr.Zero)
        {
            // throw new ArgumentException("ウィンドウハンドルが無効です。", "hWnd");
            return 0;
        }

        int dpi = GetDpiForWindow((IntPtr)hWnd);
        if (dpi == 0)
        {
            throw new Exception("DPIの取得に失敗しました");
        }
        return dpi; // XとYは同じ値になるはずです
    }
}