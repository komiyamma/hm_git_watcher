using HmNetCOM;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HmGitWatcher;

public partial class HmGitWatcher
{
    // GetParentWinndow
    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    // FindWindow
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, IntPtr none);

    // FindWindowExW の定義 (P/Invoke)
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, IntPtr none);

    // ウィンドウハンドルのクラス名を取得
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount = 256);


    // Hm.WindowHandleのクラス名。ストア版とデスクトップ版で異なため、それを考慮してキャッシュする。
    string curHmWndClassNameCache = null;

    private bool IsCurrentWindowFront()
    {
        IntPtr curHWnd = Hm.WindowHandle;

        IntPtr parentWnd = GetParent(curHWnd);
        // タブモードである
        if (parentWnd != IntPtr.Zero)
        {
            int currentWindowBackGround = Hm.Edit.InputStates & 0x00000800;
            // 非アクティブではない(=自分のプロセスはアクティブである)
            if (currentWindowBackGround == 0)
            {
                return true;
            }
        }
        // 非タブモードなら
        else
        {
            // 自身のウィンドウハンドルのクラス名のキャッシュがない
            if (curHmWndClassNameCache == null)
            {
                StringBuilder className = new StringBuilder(256);
                GetClassName(curHWnd, className, className.Capacity);
                // Hm.OutputPane.Output("className:" + className.ToString() + "\r\n");
                curHmWndClassNameCache = className.ToString();
            }

            if (curHmWndClassNameCache?.Length > 0)
            {
                // そのクラス名で検索。（非タブモードならデスクトップ直下のルートウィンドウとして秀丸ウィンドが存在する)
                IntPtr firstFindWnd = FindWindow(curHmWndClassNameCache, IntPtr.Zero);
                // 自分が秀丸の中でトップのウィンドウである
                if (firstFindWnd == curHWnd)
                {
                    // Hm.OutputPane.Output("トップウィンドウだよ\r\n");
                    return true; ;
                }
                else
                {
                    IntPtr secondFindWnd = FindWindowEx(IntPtr.Zero, firstFindWnd, curHmWndClassNameCache, IntPtr.Zero);
                    if (secondFindWnd == curHWnd)
                    {
                        // Hm.OutputPane.Output("セカンドウィンドウだよ\r\n");
                        return true; ;
                    }
                }
            }
        }

        return false;
    }

}