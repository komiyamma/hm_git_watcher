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
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, IntPtr none);

    // ウィンドウハンドルのクラス名を取得
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount = 256);


    // Hm.WindowHandleのクラス名。ストア版とデスクトップ版で異なため、それを考慮してキャッシュする。
    private string curHmWndClassNameCache = null;

    // 指定されたウィンドウのクラス名を取得する関数
    private string GetWindowClass(IntPtr hWnd)
    {
        StringBuilder className = new StringBuilder(256);
        int result = GetClassName(hWnd, className, className.Capacity);
        if (result > 0)
        {
            return className.ToString();
        }
        return null;
    }

    // この秀丸プロセスはタブモードなのか？
    private bool IsThisHmProcessTabMode(IntPtr hWnd)
    {
        // 親ウィンドウをたどり、同じクラス名のウィンドウがあるか判定する
        // 親を巡っていって同じクラス名があるのなら、それはタブモードである。
        IntPtr parentHWnd = GetParent(hWnd);
        while (parentHWnd != IntPtr.Zero)
        {
            string parentClassName = GetWindowClass(parentHWnd);
            if (parentClassName == curHmWndClassNameCache)
            {
                return true; // 同じクラス名の親ウィンドウが見つかった
            }
            parentHWnd = GetParent(parentHWnd); // さらに親をたどる
        }

        return false; // 同じクラス名の親ウィンドウは見つからなかった
    }

    private bool IsCurrentWindowFront()
    {
        IntPtr curHWnd = Hm.WindowHandle;

        // 自身のウィンドウハンドルのクラス名のキャッシュがない
        if (curHmWndClassNameCache == null)
        {
            curHmWndClassNameCache = GetWindowClass(curHWnd);
        }

        // タブモードである
        if (IsThisHmProcessTabMode(curHWnd) )
        {
            int currentWindowBackGround = Hm.Edit.InputStates & 0x00000800;
            // 自武のウィンドウはタブの裏に隠れていたり、非表示とかになっていない (=自分のプロセスはそのタブグループの中では手前にある)
            if (currentWindowBackGround == 0)
            {
                return true;
            }
        }
        // 非タブモードなら
        else
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

        return false;
    }

}