using System;
using System.Drawing;
using System.Runtime.InteropServices;
using HmNetCOM;

namespace HmGitWatcher;

public partial class HmGitWatcher
{
    Form form;
    public void ShowGitCommitForm(dynamic func)
    {
        if (form != null)
        {
            form.Close();
            form = null;
        }
        form = new GitCommitForm(func);
        form.Show();
    }
}

internal class GitCommitForm : Form
{
    TextBox commentTextBox;
    Button submitButton;

    dynamic jsCallBackFunc = null;

    // P/Invoke declaration for GetWindowRect
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    // RECT structure definition
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    double dpiScale = 1.0;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetDpiForWindow(IntPtr hWnd);

    private int GetDpiFromWindowHandle(long hWnd)
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


    public GitCommitForm(dynamic func)
    {
        jsCallBackFunc = func;
        InitDPIScale();

        InitForm();

        InitSubmitButton();
        InitCommentTextBox();

        AdjustTextBoxPositionAndSize();
        AdjustButtonPositionAndSize();
    }

    private void InitDPIScale()
    {
        // DPIスケールを取得
        int dpi = GetDpiFromWindowHandle((long)Hm.WindowHandle);
        dpiScale = dpi / 96.0;
    }

    private void InitForm()
    {

        // AutoScaleDimensions = new SizeF(7F, 15F);
        // AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size((int)(400 * dpiScale), (int)(200 * dpiScale));
        Name = "コミットのコメント";
        Text = "コミットのコメント";

        // 親ウィンドウのハンドルを取得
        IntPtr hParentWindow = (IntPtr)Hm.WindowHandle;
        if (hParentWindow != IntPtr.Zero)
        {
            // 親ウィンドウの座標を取得
            RECT rect = new RECT();
            GetWindowRect(hParentWindow, out rect);

            // rectが画面外にある。
            if (rect.Left < 0 || rect.Top < 0)
            {
                // 親ウィンドウが画面外にある場合は、画面中央に表示
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            else
            {
                // 親ウィンドウの中央に表示
                this.StartPosition = FormStartPosition.Manual;
                this.Left = rect.Left + (rect.Right - rect.Left - this.Width) / 2;
                this.Top = rect.Top + (rect.Bottom - rect.Top - this.Height) / 2;

                if (IsFormOutOfScreen())
                {
                    // 画面外に出てしまう場合は、画面中央に表示
                    this.StartPosition = FormStartPosition.CenterScreen;
                }
            }
        }

        Resize += Form1_Resize;
        Shown += Form1_Shown;
    }

    private bool IsFormOutOfScreen()
    {
        Rectangle bounds = this.Bounds;
        Rectangle screen = Screen.FromControl(this).Bounds;

        return !screen.IntersectsWith(bounds);
    }


    private void Form1_Shown(object sender, EventArgs e)
    {
        commentTextBox?.Focus();
    }

    private void InitSubmitButton()
    {
        submitButton = new Button
        {
            Location = new System.Drawing.Point((int)(12 * dpiScale), (int)(220 * dpiScale)),
            Size = new System.Drawing.Size((int)(60 * dpiScale), (int)(32 * dpiScale)),
            Name = "コミット",
            Text = "コミット"
        };
        submitButton.Click += BtnSubmit_Click;

        Controls.Add(submitButton);
    }

    private void InitCommentTextBox()
    {
        commentTextBox = new TextBox
        {
            Location = new System.Drawing.Point((int)(12 * dpiScale), (int)(12 * dpiScale)),
            Multiline = true,
            Name = "textBox1",
            Size = new System.Drawing.Size((int)(260 * dpiScale), (int)(200 * dpiScale)),
            TabIndex = 0,
            Font = new Font("MS UI Gothic", 16F, FontStyle.Regular, GraphicsUnit.Point, 128)
        };
        commentTextBox.Focus();
        commentTextBox.KeyDown += TextBox_KeyEventHandler;

        Controls.Add(commentTextBox);
    }

    private void TextBox_KeyEventHandler(object sender, KeyEventArgs e)
    {
        // リターンキーが押されていて
        if (e.KeyCode == Keys.Return)
        {
            // CTRLキーも押されている時だけ、送信ボタンを押した相当にする。
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                BtnSubmit_Click(null, e);
            }
        }
    }

    private void BtnSubmit_Click(object sender, EventArgs e)
    {
        if (jsCallBackFunc != null)
        {
            try
            {
                jsCallBackFunc(commentTextBox.Text);
            }
            catch (Exception ex)
            {
                // Hm.OutputPane.Output(ex.Message + "\r\n");
            }
            finally
            {
                commentTextBox.Text = "";
                this.Close();
            }
        }
    }

    private void Form1_Resize(object sender, EventArgs e)
    {
        // フォームリサイズ時にTextBoxの位置とサイズを調整
        AdjustTextBoxPositionAndSize();
        AdjustButtonPositionAndSize();
    }

    private void AdjustTextBoxPositionAndSize()
    {
        // パディング値
        int padding = (int)(10 * dpiScale);

        // TextBoxの新しい位置とサイズを計算
        int x = padding;
        int y = padding;
        int width = this.ClientSize.Width - 2 * padding;
        int height = this.ClientSize.Height - 2 * padding - (int)(40 * dpiScale);

        // TextBoxの位置とサイズを設定
        this.commentTextBox.Location = new Point(x, y);
        this.commentTextBox.Size = new Size(width, height);
    }

    private void AdjustButtonPositionAndSize()
    {
        // ボタンの位置を計算
        int x = (this.ClientSize.Width - submitButton.Width) / 2;
        int y = this.ClientSize.Height - submitButton.Height - (int)(10 * dpiScale); // 下部からのマージンを10とする

        this.submitButton.Location = new Point(x, y);
    }
}
