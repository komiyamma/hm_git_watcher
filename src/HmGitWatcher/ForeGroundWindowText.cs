using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HmNetCOM;

namespace HmGitWatcher;

public partial class HmGitWatcher
{

    // WinAPIの関数をインポート
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder lpString, int nMaxCount);

    // 最大のウィンドウキャプション文字数
    private const int MaxTitleLength = 256;

    public string GetForegroundWindowText()
    {
        // アクティブウィンドウのハンドルを取得
        IntPtr hwnd = GetForegroundWindow();

        // ウィンドウのタイトルを取得
        System.Text.StringBuilder title = new System.Text.StringBuilder(MaxTitleLength);
        GetWindowText(hwnd, title, MaxTitleLength);

        // Hm.OutputPane.Output(title.ToString() + "★\r\n");

        return title.ToString();
    }
}