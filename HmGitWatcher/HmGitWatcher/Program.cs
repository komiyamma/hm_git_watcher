using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HmNetCOM;

namespace HmGitWatcher;

[Guid("55D951B6-A12E-41F5-BB6E-67F295C2F3DA")]
class HmGitWatcher
{
    /// <summary>
    /// 重い処理を非同期で実行するメソッド
    /// </summary>
    /// <returns></returns>
    private async Task RunHeavyProcessAsync()
    {
        Console.WriteLine("重い処理を開始します...");

        // ここに重い処理を記述 (例: CPU負荷の高い計算、ネットワーク通信など)
        await Task.Delay(2000); // 2秒間の遅延で代替

        Console.WriteLine("重い処理が完了しました。");
    }

    /// <summary>
    /// 重い処理を繰り返し実行するメソッド
    /// </summary>
    private async Task StartRepeatingProcessAsync()
    {
        while (true) // 無限ループで繰り返し
        {
            await RunHeavyProcessAsync();
            Console.WriteLine("3秒間待機します。");
            await Task.Delay(3000); // 3秒待機
            Console.WriteLine("次の処理を開始します。");
        }
    }

    private string gitExecutablePath = "git"; // gitのフルパス

    private async Task Start()
    {
        if (jsFunction == null)
        {
            Hm.OutputPane.Output("javascriptのコールバック用関数が引き渡されていません\r\n");
        }

        // 第１引数があれば、それはgitのフルパスとして保持
        if (args.Length > 0)
        {
            if (System.IO.File.Exists(args[0]))
            {
                gitExecutablePath = args[0];
            }
            else
            {
                Console.WriteLine("指定されたgitのフルパスが存在しません。");
                return;
            }
            Console.WriteLine(args[0]);
        }

        StartRepeatingProcessAsync();

        Console.WriteLine("終了しました"); // ここはたどり着かない
    }

    Object jsFunction = null;

    public int startGitWatcher(string gitFullpath, object jsFunc)
    {
        this.gitExecutablePath = gitFullpath;
        this.jsFunction = jsFunc;

        Start();
    }
}
}
