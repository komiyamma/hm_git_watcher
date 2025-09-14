using HmNetCOM;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HmGitWatcher;

#if (NET || NETCOREAPP3_1)
[ComVisible(true)]
[Guid("CD5AADB6-1A50-436F-85A1-84D72CFAECEB")]
#else
[ComVisible(true)]
[Guid("80319443-F6CF-4526-84BE-D4CBECEE70CB")]
#endif

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
                    /*
                    Hm.OutputPane.Output(startInfo.FileName + startInfo.Arguments + "実行中にエラーが発生しました: {error} + \r\n");
                    */
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
            Hm.OutputPane.Output(startInfo?.FileName + " " + startInfo?.Arguments + "実行中にエラーが発生しました: {ex} + \r\n");
            return null;
        }
    }

    string GetGitFetch(string workingDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "fetch",
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
            string stdOutSum = "";
            string stdErrSum = "";

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        stdOutSum += args.Data;
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        stdOutSum += args.Data;
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return stdOutSum;
            }
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo?.FileName + " " + startInfo?.Arguments + $"実行中にエラーが発生しました: {ex} + \r\n");
            Stop();
            return null; // エラー発生時もnullを返す
        }
    }

    string GetGitStatus(string workingDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "status",
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
            string stdOutSum = "";
            string stdErrSum = "";

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        stdOutSum += args.Data;
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        // Hm.OutputPane.Output(args.Data + "\r\n");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return stdOutSum;
            }
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo?.FileName + " " + startInfo?.Arguments + $"実行中にエラーが発生しました: {ex} + \r\n");
            return null; // エラー発生時もnullを返す
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
            string stdOutSum = "";
            string stdErrSum = "";

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        stdOutSum += args.Data;
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        // Hm.OutputPane.Output(args.Data + "\r\n");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return stdOutSum;
            }
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo?.FileName + " " + startInfo?.Arguments + $"実行中にエラーが発生しました: {ex} + \r\n");
            return null; // エラー発生時もnullを返す
        }
    }


    string GetGitStatusPorchain(string workingDirectory)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "status --porcelain -z",
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
            string stdOutSum = "";
            string stdErrSum = "";


            using (Process process = new Process())
            {

                process.StartInfo = startInfo;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        // ナル文字('\0')で分割
                        string[] parts = args.Data.Split('\0');
                        foreach (var part in parts)
                        {
                            if (part.Length > 0)
                            {
                                stdOutSum += part + "\r\n";
                            }
                        }
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        /*
                        // ナル文字('\0')で分割
                        string[] parts = args.Data.Split('\0');
                        foreach (var part in parts)
                        {
                            if (part.Length > 0)
                            {
                                stdOutSum += part + "\r\n";
                            }
                        }
                        */
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return stdOutSum;
            }
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo?.FileName + " " + startInfo?.Arguments + $"実行中にエラーが発生しました: {ex} + \r\n");
            return null; // エラー発生時もnullを返す
        }
    }


    string prevStatus = "";
    string prevPorchain = "";
    string prevCherry = "";
    string prevRepoPath = "";
    string prevFilePath = "";

    private bool isChangeNotify = false;

    private async Task CheckInternal(dynamic callBackFoundRepos, dynamic callBackStatusChange, CancellationToken cancellationToken)
    {
        // Hm.OutputPane.Output("▼タスク開始------------------\r\n");
        while (!cancellationToken.IsCancellationRequested)
        {
            if (renderpaneCommand)
            {
                try
                {
                    if (callBackStatusChange == null)
                    {
                        // Hm.OutputPane.Output("★callBackStatusChange is null & STOP()\r\n");
                        Stop();
                        break;
                    }

                    string filePath = Hm.Edit.FilePath;

                    if (string.IsNullOrEmpty(filePath))
                    {
                        // callBackFunc("", "", "");
                        prevStatus = "";
                        prevPorchain = "";
                        prevCherry = "";
                        prevRepoPath = "";
                        prevFilePath = "";
                        // Hm.OutputPane.Output("★IsNullOrEmpty & STOP()\r\n");
                        Stop();
                        break;
                    }

                    if (prevFilePath != filePath)
                    {
                        prevStatus = "";
                        prevPorchain = "";
                        prevCherry = "";
                        prevRepoPath = "";
                        prevFilePath = "";
                        // Hm.OutputPane.Output("★(prevFilePath != filePath) & STOP()\r\n");
                        Stop();
                        break;
                    }

                    string repoPath = GetAbsoluteGitDir(filePath);
                    if (String.IsNullOrEmpty(repoPath))
                    {
                        try
                        {
                            // Hm.OutputPane.Output("■callBackStatusChange(Empty Args) & STOP()\r\n");
                            callBackStatusChange("", "", "");
                            Stop();
                        }
                        catch (Exception ex)
                        {
                            // Hm.OutputPane.Output("■callBackStatusChange(Empty Args) raise Exception & STOP()\r\n");
                            Stop();
                            break;
                        }
                        prevStatus = "";
                        prevPorchain = "";
                        prevCherry = "";
                        prevRepoPath = "";
                        prevFilePath = "";
                    }

                    else
                    {
                        if (prevRepoPath != repoPath)
                        {
                            try
                            {
                                callBackFoundRepos(repoPath ?? "");
                            }
                            catch (Exception ex)
                            {
                            }


                            // Hm.OutputPane.Output("★FileWatcher ReCreate\r\n");
                            ReCreateFileWatcher(repoPath);
                        }
                        GetGitFetch(repoPath);
                        string status = GetGitStatus(repoPath);
                        string porchain = GetGitStatusPorchain(repoPath);
                        string cherry = GetGitCherry(repoPath);

                        if (prevStatus == status && prevPorchain == porchain && prevCherry == cherry && prevRepoPath == repoPath)
                        {
                            // 変更なし
                        }
                        else
                        {
                            // Hm.OutputPane.Output("変更がありました。");
                            prevStatus = status;
                            prevPorchain = porchain;
                            prevCherry = cherry;
                            prevRepoPath = repoPath;

                            string lastCheckFilePath = Hm.Edit.FilePath;
                            if (!string.IsNullOrEmpty(lastCheckFilePath))
                            {
                                try
                                {
                                    // Hm.OutputPane.Output("■callBackStatusChange(...) 変化をJSに反映\r\n");
                                    // 実行先が存在しないことが考えられる。
                                    callBackStatusChange(repoPath ?? "", status ?? "", porchain ?? "", cherry ?? "");
                                }
                                catch (Exception ex)
                                {
                                    // Hm.OutputPane.Output("■callBackStatusChange(...) raise Exception & STOP()\r\n");
                                    Stop();
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Hm.OutputPane.Output($"Gitリポジトリ調査中にエラー発生: {ex.Message}");
                }

                // 最大で600秒待機
                for (int i = 0; i < 6000*2; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Hm.OutputPane.Output("★IsCancellationRequested\r\n");
                        break;
                    }
                    if (isChangeNotify)
                    {
                        // Hm.OutputPane.Output("isChangeNotifyを検知したので、タイムを短縮");
                        isChangeNotify = false;
                        break;
                    }
                    if (i > 100*2 && IsCurrentWindowOperate()) // 10秒以上経過で、操作しているウィンドウ
                    {
                        break;
                    }

                    /*
                    int layer = IsCurrentWindowFront(); // 1が手前に見えてる、２は見えてるかわからんが２番手
                    if (i > 40*2 && layer == 1) // 40秒以上経過で、前面もしくはそれ相当
                    { 
                        break;
                    }
                    */

                    /*
                    if (i > 90*2 && layer == 2) // 90秒以上経過で、２番手
                    {
                        break;
                    }
                    */

                    await Task.Delay(1000/2, cancellationToken); // 0.5秒間隔
                }
            }

            else // renderpaneCommand)
            {
                await Task.Delay(500, cancellationToken); // 0.5秒間隔
            }
        }

        // Hm.OutputPane.Output("▲タスク終了------------------\r\n");
    }


    private CancellationTokenSource _cancellationTokenSource;
    private void StartCheck(dynamic callBackFoundRepos, dynamic callBackStatusChange)
    {
        try
        {
            renderpaneCommand = false;

            prevFilePath = Hm.Edit.FilePath;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            // 非同期処理を開始
            Task.Run(async () => await CheckInternal(callBackFoundRepos, callBackStatusChange, cancellationToken));
        }
        catch (Exception ex)
        {
        }
        // Hm.OutputPane.Output("Git監視を開始しました。\r\n");
    }


    public void ReStart(dynamic callBackFoundRepos, dynamic callBackStatusChange)
    {
        try
        {
            // すでに監視を開始している場合は停止する
            Stop();
            // Hm.OutputPane.Output("\r\n\r\n▲STOP() & ▼ReStart()\r\n");
            StartCheck(callBackFoundRepos, callBackStatusChange);
        }
        catch (Exception ex)
        {
        }
    }

    public void Stop()
    {
        try
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                // Hm.OutputPane.Output("Git監視を停止しました。\r\n");
            }
        }
        catch (Exception ex)
        {
        }

        try
        {
            DesposeFileWatcher();
        }
        catch (Exception ex)
        {
        }

        try
        {
            StopDPIWatcher();
        }
        catch (Exception ex)
        {
        }

        try
        {
            if (gitcomment_form != null)
            {
                gitcomment_form.Close();
                gitcomment_form = null;
            }
        }
        catch (Exception ex)
        {
        }

        try
        {
            if (messagebox_form != null)
            {
                messagebox_form.Close();
                messagebox_form = null;
            }
        }
        catch (Exception ex)
        {
        }

        try
        {
            if (popup_menu != null)
            {
                popup_menu.Dispose();
                popup_menu = null;
            }
        }
        catch (Exception ex)
        {
        }
    }

    public void ChangeNotify()
    {
        isChangeNotify = true;
    }

    bool renderpaneCommand = false;
    public void sendCommandRenderPaneCommand()
    {
        renderpaneCommand = true;
    }
}