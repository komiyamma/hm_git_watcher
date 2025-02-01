using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmNetCOM;
namespace HmGitWatcher;

using System;
using System.Drawing;
using System.Windows.Forms;

public partial class HmGitWatcher
{
    static PopupMenuAsync popup_menu;

    public void ShowAsyncPopupMenu(dynamic func)
    {
        if (popup_menu != null)
        {
            popup_menu.Dispose();
            popup_menu = null;
        }


        popup_menu = new PopupMenuAsync();

        // 例として、特定の場所にポップアップメニューを表示
        var x = Hm.Edit.MousePos.X;
        var y = Hm.Edit.MousePos.Y;
        popup_menu.ShowPopupMenu(new Point(x, y));
    }
}

internal class PopupMenuAsync
{
    private ContextMenuStrip contextMenu;

    public PopupMenuAsync()
    {
        // コンテキストメニューの新規作成
        contextMenu = new ContextMenuStrip();

        // メニュー項目の追加
        ToolStripMenuItem item1 = new ToolStripMenuItem("Item 1");
        item1.Click += (sender, e) => {
            Console.WriteLine("Item 1 clicked");
            // ここにItem 1がクリックされた時の処理を記述
        };
        contextMenu.Items.Add(item1);

        ToolStripMenuItem item2 = new ToolStripMenuItem("Item 2");
        item2.Click += (sender, e) => {
            Console.WriteLine("Item 2 clicked");
            // ここにItem 2がクリックされた時の処理を記述
        };
        contextMenu.Items.Add(item2);

        // 区切り線の追加
        contextMenu.Items.Add(new ToolStripSeparator());

        ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (sender, e) => {
            // アプリケーション終了の処理（フォーム不要）
            Console.WriteLine("Exit selected, but console will not exit automatically.");
            // 必要であれば、ここでアプリケーションの終了に関する処理を記述
        };
        contextMenu.Items.Add(exitItem);
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
