using System;
using System.Drawing;
using HmNetCOM;

namespace HmGitWatcher;

public partial class HmGitWatcher
{
    Form form;
    public void ShowGitCommitForm(dynamic func)
    {
        form = new GitCommitForm(func);
        form.Show();
    }
}

internal class GitCommitForm : Form
{
TextBox commentTextBox;
Button submitButton;

dynamic jsCallBackFunc = null;

public GitCommitForm(dynamic func)
    {
        jsCallBackFunc = func;

        InitForm();

        InitSubmitButton();
        InitCommentTextBox();

        AdjustTextBoxPositionAndSize();
        AdjustButtonPositionAndSize();
    }

    private void InitForm()
    {
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 200);
        Name = "コミットのコメント";
        Text = "コミットのコメント";

        Resize += Form1_Resize;
        Shown += Form1_Shown;
    }

    private void Form1_Shown(object sender, EventArgs e)
    {
        commentTextBox?.Focus();
    }

    private void InitSubmitButton()
    {
        submitButton = new Button
        {
            Location = new System.Drawing.Point(12, 220),
            Size = new System.Drawing.Size(60, 32),
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
            Location = new System.Drawing.Point(12, 12),
            Multiline = true,
            Name = "textBox1",
            Size = new System.Drawing.Size(260, 200),
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
    int padding = 10;

    // TextBoxの新しい位置とサイズを計算
    int x = padding;
    int y = padding;
    int width = this.ClientSize.Width - 2 * padding;
    int height = this.ClientSize.Height - 2 * padding - 40;

    // TextBoxの位置とサイズを設定
    this.commentTextBox.Location = new Point(x, y);
    this.commentTextBox.Size = new Size(width, height);
}

private void AdjustButtonPositionAndSize()
{
    // ボタンの位置を計算
    int x = (this.ClientSize.Width - submitButton.Width) / 2;
    int y = this.ClientSize.Height - submitButton.Height - 10; // 下部からのマージンを10とする

    this.submitButton.Location = new Point(x, y);
}
}
