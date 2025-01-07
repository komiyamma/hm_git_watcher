using System;
using System.Drawing;

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
TextBox textBox1;
Button btnSubmit;

dynamic jsCallBackFunc = null;

public GitCommitForm(dynamic func)
{
    jsCallBackFunc = func;

    btnSubmit = new Button();
    btnSubmit.Location = new System.Drawing.Point(12, 220);
    btnSubmit.Size = new System.Drawing.Size(60, 32);
    btnSubmit.Name = "コミット";
    btnSubmit.Text = "コミット";
    btnSubmit.Click += btnSubmit_Click;


    textBox1 = new TextBox();
    textBox1.Location = new System.Drawing.Point(12, 12);
    textBox1.Multiline = true;
    textBox1.Name = "textBox1";
    textBox1.Size = new System.Drawing.Size(260, 200);
    textBox1.TabIndex = 0;
    textBox1.Font = new Font("MS UI Gothic", 16F, FontStyle.Regular, GraphicsUnit.Point, 128);

    AutoScaleDimensions = new SizeF(7F, 15F);
    AutoScaleMode = AutoScaleMode.Font;
    ClientSize = new Size(800, 450);
    Name = "コミットのコメント";
    Text = "コミットのコメント";

    Resize += Form1_Resize;

    Controls.Add(textBox1);
    Controls.Add(btnSubmit);

    AdjustTextBoxPositionAndSize();
    AdjustButtonPosition(); // 初期配置のために呼び出し
}

private void btnSubmit_Click(object sender, EventArgs e)
{
    if (jsCallBackFunc != null)
    {
        try
        {
            jsCallBackFunc(textBox1.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            textBox1.Text = "";
            this.Close();
        }
    }
}

private void Form1_Resize(object sender, EventArgs e)
{
    // フォームリサイズ時にTextBoxの位置とサイズを調整
    AdjustTextBoxPositionAndSize();
    AdjustButtonPosition();
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
    this.textBox1.Location = new Point(x, y);
    this.textBox1.Size = new Size(width, height);
}

private void AdjustButtonPosition()
{
    // ボタンの位置を計算
    int x = (this.ClientSize.Width - btnSubmit.Width) / 2;
    int y = this.ClientSize.Height - btnSubmit.Height - 10; // 下部からのマージンを10とする

    this.btnSubmit.Location = new Point(x, y);
}
}
