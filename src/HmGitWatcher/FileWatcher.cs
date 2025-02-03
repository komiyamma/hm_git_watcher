using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HmNetCOM;

namespace HmGitWatcher;


public partial class HmGitWatcher
{
    private FileSystemWatcher watcher;

    private void ReCreateFileWatcher(string targetPath)
    {
        DesposeFileWatcher();
        watcher = new FileSystemWatcher();
        watcher.Path = targetPath;
        watcher.IncludeSubdirectories = true; // サブディレクトリも監視
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
        // 隠しファイルは対象にしない

        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileChanged;
        watcher.Deleted += OnFileChanged;
        watcher.Renamed += OnFileChanged;

        watcher.EnableRaisingEvents = true;
    }

    private static bool IsUnderHiddenDirectory(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                return (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            }

            else if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                return (dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            }

        }
        catch (Exception)
        {
        }

        return false;
    }


    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            if (e.FullPath.Contains("\\.git\\") || e.FullPath.EndsWith("\\.git"))
            {
                return;
            }

            if (IsUnderHiddenDirectory(e.FullPath))
            {
                return;
            }

            // イベントが発生した場合は変更フラグを立てる
            isChangeNotify = true;
        }
        catch (Exception ex)
        {
        }

        // Hm.OutputPane.Output(e?.FullPath + "\r\n");
        // Hm.OutputPane.Output("OnFileChanged" + "\r\n");
    }

    private void OnFileChanged(object sender, RenamedEventArgs e)
    {
        try
        {
            if (e.FullPath.Contains("\\.git\\") || e.FullPath.EndsWith("\\.git"))
            {
                return;
            }

            if (IsUnderHiddenDirectory(e.FullPath))
            {
                return;
            }

            // イベントが発生した場合は変更フラグを立てる
            isChangeNotify = true;

        }
        catch (Exception ex)
        {
        }

        // Hm.OutputPane.Output(e?.FullPath + "\r\n");
        // Hm.OutputPane.Output("OnFileChanged" + "\r\n");
    }
    public void DesposeFileWatcher()
    {
        if (watcher != null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
    }
}
