﻿using HmNetCOM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmGitWatcher;

public partial class HmGitWatcher
{
    public string CommandGitCommit(string workingDirectory, string comment, dynamic stdOutFunc, dynamic stdErrFunc)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "commit -z -m " + comment,
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
            Hm.OutputPane.Output(startInfo.Arguments + "\r\n");

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
                        // ナル文字('\0')で分割
                        string[] parts = args.Data.Split('\0');
                        foreach (var part in parts)
                        {
                            if (part.Length > 0)
                            {
                                stdErrSum += part + "\r\n";
                            }
                        }
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                if (stdOutFunc != null)
                {
                    try
                    {
                        stdOutFunc(stdOutSum);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if (stdErrFunc != null)
                {
                    try
                    {
                        stdErrFunc(stdErrSum);
                    }
                    catch (Exception ex)
                    {
                    }
                }

                return stdOutSum;
            }
        }
        catch (Exception ex)
        {
            Hm.OutputPane.Output(startInfo.FileName + " " + startInfo.Arguments + $"実行中にエラーが発生しました: {ex} + \r\n");
            return null; // エラー発生時もnullを返す
        }
    }
}
