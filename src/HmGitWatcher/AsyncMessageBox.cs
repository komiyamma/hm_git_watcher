using HmNetCOM;
using System.Runtime.InteropServices;

namespace HmGitWatcher;

public partial class HmGitWatcher
{
    static Form messagebox_form;
    public void ShowAsyncMessageBox(dynamic func, string title, string message, string button_text)
    {
        if (messagebox_form != null)
        {
            messagebox_form.Close();
            messagebox_form = null;
        }
        messagebox_form = new AsyncMessageBoxForm(func, title, message, button_text);
        messagebox_form.Show();
    }
}

internal partial class AsyncMessageBoxForm : Form
{
    // ----------------------------------------------------------------
    // GetWindowRect用。hidemaru.getCurrentWindowHandle() 相当のRECT取得用
    // ----------------------------------------------------------------
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);



    // ----------------------------------------------------------------
    // GetDpiForWindow用。hidemaru.getCurrentWindowHandle() 相当のDPI取得用
    // ----------------------------------------------------------------
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetDpiForWindow(IntPtr hWnd);

    private static double GetDpiScaleFromWindowHandle(long hWnd)
    {
        if ((IntPtr)hWnd == IntPtr.Zero)
        {
            // throw new ArgumentException("ウィンドウハンドルが無効です。", "hWnd");
            return 1;
        }

        uint dpi = GetDpiForWindow((IntPtr)hWnd);
        if (dpi == 0)
        {
            // throw new Exception("DPIの取得に失敗しました");
            return 1;
        }

        // 正しく取れたようだ
        return dpi / 96;
    }

    // ----------------------------------------------------------------
    // エディタ編集のハンドルを取得する。
    // ----------------------------------------------------------------
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr hWnd);

    private static IntPtr GetChildWindowHandleByClassName(IntPtr parentHandle, string className)
    {
        IntPtr childHandle = IntPtr.Zero;
        IntPtr foundHandle = IntPtr.Zero;

        // 子ウィンドウを順に検索し、指定されたクラス名を持つウィンドウを探す
        while ((foundHandle = FindWindowEx(parentHandle, childHandle, className, null)) != IntPtr.Zero)
        {
            if (IsWindow(foundHandle))
            {
                return foundHandle; // 見つかったら返す
            }
            childHandle = foundHandle; // 次の検索のため更新
        }

        return IntPtr.Zero; // 見つからなかったら IntPtr.Zero を返す
    }
};


internal partial class AsyncMessageBoxForm : Form
{
    Label label;

    Button submitButton;

    dynamic jsCallBackFunc = null;

    double dpiScale = 1.0;

    public AsyncMessageBoxForm(dynamic func, string title, string message, string button_text)
    {
        jsCallBackFunc = func;
        InitDPIScale();

        InitForm(title);

        InitSubmitButton(button_text);
        InitLabel(message);

        AdjustLabelPositionAndSize();
        AdjustButtonPositionAndSize();
    }

    private void InitDPIScale()
    {
        // DPIスケールを取得
        dpiScale = GetDpiScaleFromWindowHandle((long)Hm.WindowHandle);
    }

    private void InitForm(String title)
    {

        // AutoScaleDimensions = new SizeF(7F, 15F);
        // AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size((int)(400 * dpiScale), (int)(200 * dpiScale));
        Name = "タイトル";
        Text = title;

        // 親ウィンドウのハンドルを取得(これは絶対にある。ただしウィンドウは非表示かも？)
        IntPtr hParentWindow = (IntPtr)Hm.WindowHandle;
        if (hParentWindow != IntPtr.Zero)
        {
            // 親ウィンドウの座標を取得
            RECT rect = new RECT();
            GetWindowRect(hParentWindow, out rect);

            // 親ウィンドウの中央に表示
            this.StartPosition = FormStartPosition.Manual;
            this.Left = rect.Left + (rect.Right - rect.Left - this.Width) / 2;
            this.Top = rect.Top + (rect.Bottom - rect.Top - this.Height) / 2;
        }

        // 編集エリアのハンドルを取得(これは理論上はあるハズだが、絶対とまでは言えないかもしれない。またウィンドウは非表示かも？)
        IntPtr hEditWindow = GetChildWindowHandleByClassName(hParentWindow, "HM32CLIENT");
        if (hEditWindow != IntPtr.Zero)
        {
            // 親ウィンドウの座標を取得
            RECT rect = new RECT();
            GetWindowRect(hEditWindow, out rect);
            // rectが画面外にある。
            this.StartPosition = FormStartPosition.Manual;
            this.Left = rect.Left + (rect.Right - rect.Left - this.Width) / 2;
            this.Top = rect.Top + (rect.Bottom - rect.Top - this.Height) / 2;
        }

        Resize += Form_OnResize;
        Shown += Form1_Shown;
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
        label?.Focus();
    }

    private void InitSubmitButton(string button_text)
    {
        if (String.IsNullOrEmpty(button_text))
        {
            button_text = "OK";
        }

        submitButton = new Button
        {
            Location = new System.Drawing.Point((int)(12 * dpiScale), (int)(220 * dpiScale)),
            Size = new System.Drawing.Size((int)(60 * dpiScale), (int)(32 * dpiScale)),
            TabIndex = 0,
            Name = "コミット",
            Text = button_text
        };

        submitButton.Focus();

        submitButton.Click += BtnSubmit_Click;

        Controls.Add(submitButton);
    }

    private void InitLabel(string message)
    {
        label = new Label
        {
            Location = new System.Drawing.Point((int)(12 * dpiScale), (int)(12 * dpiScale)),
            Name = "label1",
            Text = message,
            Size = new System.Drawing.Size((int)(260 * dpiScale), (int)(200 * dpiScale)),
            Font = new Font("MS UI Gothic", 16F, FontStyle.Regular, GraphicsUnit.Point, 128)
        };

        Controls.Add(label);
    }


    private void BtnSubmit_Click(object sender, EventArgs e)
    {
        if (jsCallBackFunc != null)
        {
            try
            {
                // Hm.OutputPane.Output(jsCallBackFunc + "\r\n");
                jsCallBackFunc(this.Text, label.Text, submitButton.Text);
            }
            catch (Exception ex)
            {
                // Hm.OutputPane.Output(ex.Message + "\r\n");
            }
            finally
            {
                label.Text = "";
                submitButton.Text = "OK";
                this.Close();
            }
        }

    }

    private void Form_OnResize(object sender, EventArgs e)
    {
        // フォームリサイズ時にTextBoxの位置とサイズを調整
        AdjustLabelPositionAndSize();
        AdjustButtonPositionAndSize();
    }

    private void AdjustLabelPositionAndSize()
    {
        // パディング値
        int padding = (int)(10 * dpiScale);

        // TextBoxの新しい位置とサイズを計算
        int x = padding;
        int y = padding;
        int width = this.ClientSize.Width - 2 * padding;
        int height = this.ClientSize.Height - 2 * padding - (int)(40 * dpiScale);

        // TextBoxの位置とサイズを設定
        this.label.Location = new Point(x, y);
        this.label.Size = new Size(width, height);
    }

    private void AdjustButtonPositionAndSize()
    {
        // ボタンの位置を計算
        int x = (this.ClientSize.Width - submitButton.Width) / 2;
        int y = this.ClientSize.Height - submitButton.Height - (int)(10 * dpiScale); // 下部からのマージンを10とする

        this.submitButton.Location = new Point(x, y);
    }

}