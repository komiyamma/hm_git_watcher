using HmNetCOM;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace HmGitWatcher;


public partial class HmGitWatcher
{
    static PopupMenuAsync popup_menu;

    public void ShowAsyncPopupMenu(dynamic func, string menu01, string menu02 = null, string menu03 = null, string menu04 = null, string menu05 = null, string menu06 = null, string menu07 = null, string menu08 = null, string menu09 = null)

    {
        try
        {
            // menu01～ menu09 までの引数を配列に格納
            string[] menuArray = { menu01, menu02, menu03, menu04, menu05, menu06, menu07, menu08, menu09 };
            // nullなものはカット
            if (menuArray == null)
            {
                Hm.OutputPane.Output("メニューへの引数がnullです。\r\n");
                return;
            }

            if (menuArray.Length == 0)
            {
                Hm.OutputPane.Output("メニューへの引数が配列が空です。\r\n");
            }

            if (popup_menu != null)
            {
                popup_menu.Dispose();

                popup_menu = null;
            }

            popup_menu = new PopupMenuAsync(func, menuArray);

            // 例として、特定の場所にポップアップメニューを表示
            var x = Hm.Edit.MousePos.X;
            var y = Hm.Edit.MousePos.Y;

            popup_menu.ShowPopupMenu(new Point(x, y));
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(ex.Message + "\r\n");
        }
    }
}

internal class PopupMenuAsync
{
    private ContextMenuStrip contextMenu;

    private string[] menuArray;

    private dynamic jsCallBackFunc;

    public PopupMenuAsync(dynamic func, string[] menu_array)
    {
        if (func == null)
        {
            return;
        }

        if (menu_array == null)
        {
            return;
        }

        this.jsCallBackFunc = func;

        this.menuArray = menu_array;

        // コンテキストメニューの新規作成
        contextMenu = new ContextMenuStrip();

        foreach (var menuString in menuArray)
        {
            if (menuString == null)
            {
                continue;
            }
            if (menuString == "---")
            {
                contextMenu.Items.Add(new ToolStripSeparator());
                continue;
            }

            ToolStripMenuItem item = new ToolStripMenuItem(menuString);
            item.Click += On_PopupMenuItem_Click;

            contextMenu.Items.Add(item);
        }
    }

    private void On_PopupMenuItem_Click(object sender, EventArgs e)
    {
        // senderをToolStripMenuItem型にキャスト
        ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;

        // 親のContextMenuStripを取得
        ContextMenuStrip parentMenu = (ContextMenuStrip)clickedItem.Owner;

        // メニュー項目のコレクションから、クリックされた要素のインデックスを取得
        int index = parentMenu.Items.IndexOf(clickedItem);

        try
        {
            jsCallBackFunc(index, clickedItem?.Text ?? "");
        }
        catch (Exception ex)
        {
            // Hm.OutputPane.Output(ex.Message + "\r\n");
        }
        finally
        {
            contextMenu.Text = ""; // この無意味な記述によりGUIスレッドを使う。これにより最適化してもjsCallBackFuncの終わりを待つことが保証されるという点で意味がある。
        }
        // ここでインデックスを使った処理を行う
        // Hm.OutputPane.Output($"「{clickedItem.Text}」がクリックされました。インデックス: {index}\r\n");
    }

    // ポップアップメニューを表示するメソッド
    public void ShowPopupMenu(Point location)
    {
        contextMenu.Show(location);
    }

    public void Dispose()
    {
        contextMenu.Dispose();
    }
}
