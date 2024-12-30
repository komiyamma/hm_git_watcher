using HmNetCOM;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace HmGitWatcher;


[ComVisible(true)]
[Guid("CD5AADB6-1A50-436F-85A1-84D72CFAECEB")]
public partial class HmGitWatcher
{

    string GetAbsoluteGitDir(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "rev-parse --absolute-git-dir",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = directory,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {

            using (Process process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    return null; // プロセスが起動できなかった
                }


                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    Hm.OutputPane.Output(startInfo.FileName + startInfo.Arguments + "実行中にエラーが発生しました: {error} + \r\n");
                    return null; // git コマンドがエラー
                }

                string path = process.StandardOutput.ReadToEnd();
                path = path.Trim();

                if (path.EndsWith("/.git")) // 文字列が指定された接尾辞で終わっているか確認
                {
                    return Path.GetDirectoryName(path);
                }
                return path;
            }

        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo.FileName + startInfo.Arguments + "実行中にエラーが発生しました: {ex} + \r\n");
            return null;
        }
    }
    string GetGitCherry(string workingDirectory)
    {

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "cherry -v",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {

            using (Process process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    return null; // プロセスが起動できなかった
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    Hm.OutputPane.Output(startInfo.FileName + startInfo.Arguments + "実行中にエラーが発生しました: {error} + \r\n");
                    return null; // git コマンドがエラー
                }


                string output = process.StandardOutput.ReadToEnd();
                return output;
            }
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo.FileName + startInfo.Arguments + "実行中にエラーが発生しました: {ex} + \r\n");
            return null;
        }
    }


    string GetGitStatusPorchain(string workingDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "status --porcelain",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false, // ShellExecute を false にしてリダイレクトを有効にする
            CreateNoWindow = true,  // コンソールウィンドウを表示しない
            StandardOutputEncoding = Encoding.UTF8, // 出力エンコーディングをUTF-8に指定
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {
            using (Process process = Process.Start(startInfo))
            {

                if (process == null)
                {
                    return null; // プロセスが起動できなかった
                }

                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    Hm.OutputPane.Output(startInfo.FileName + startInfo.Arguments + "実行中にエラーが発生しました: {error} + \r\n");
                    return null; // エラーが発生した場合、nullを返す
                }

                string output = process.StandardOutput.ReadToEnd();
                return output;
            }
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo.FileName + startInfo.Arguments + "実行中にエラーが発生しました: {ex} + \r\n");
            return null; // エラー発生時もnullを返す
        }
    }

    string prevStatus = "";
    string prevCherry = "";
    string prevRepoPath = "";

    private async Task CheckInternal(dynamic callBackFunc, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (callBackFunc == null)
                {
                    Hm.OutputPane.Output("コールバック関数が指定されていません。");
                    return;
                }

                string filePath = Hm.Edit.FilePath;

                if (string.IsNullOrEmpty(filePath))
                {
                    callBackFunc("", "", "");
                    prevStatus = "";
                    prevCherry = "";
                    prevRepoPath = "";
                }
                else
                {
                    string repoPath = GetAbsoluteGitDir(filePath);
                    if (repoPath != null)
                    {
                        if (prevRepoPath != repoPath)
                        {
                            CreateFileWatcher(repoPath);
                        }
                        string status = GetGitStatusPorchain(repoPath);
                        string cherry = GetGitCherry(repoPath);
                        // Hm.OutputPane.Output($"Status: {status}\r\n\r\n");
                        if (prevStatus != status || prevCherry != cherry || prevRepoPath != repoPath)
                        {
                            Hm.OutputPane.Output("変更がありました。");
                            prevStatus = status;
                            prevCherry = cherry;
                            prevRepoPath = repoPath;
                            callBackFunc(repoPath, status, cherry);

                        }
                        else
                        {
                            Hm.OutputPane.Output("変更はありません。");
                        }
                    }
                    else
                    {
                        // Hm.OutputPane.Output($"Repository root path: {repoPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Hm.OutputPane.Output($"An error occurred: {ex.Message}");
            }

            for(int i = 0; i < 4; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(1000, cancellationToken); // 1秒間隔
                if (isFileChanged)
                {
                    Hm.OutputPane.Output("isFileChangedを検知したので、タイムを短縮");
                    isFileChanged = false;
                    break;
                }
            }
        }

    }
    private CancellationTokenSource _cancellationTokenSource;
    public void StartCheck(dynamic callBackFunc)
    {
        // すでに監視を開始している場合は停止する
        StopCheck();

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        // 非同期処理を開始
        Task.Run(async () => await CheckInternal(callBackFunc, cancellationToken));

        Hm.OutputPane.Output("Git監視を開始しました。");
    }

    public void StopCheck()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
            Hm.OutputPane.Output("Git監視を停止しました。");
        }
    }

    public void Check(dynamic callBackFunc)
    {
        StartCheck(callBackFunc);
    }
}