using System;
using System.IO;

namespace HmGitWatcher;


public partial class HmGitWatcher
{
    private static FileSystemWatcher watcher;

    private void ReCreateFileWatcher(string targetPath)
    {
        try
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
        catch (Exception ex)
        {
        }
    }

    private bool IsUnderHiddenDirectory(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                if ((fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    return true;
                }
            }

            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                if ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    return true;
                }
            }

            var parent = new DirectoryInfo(Path.GetDirectoryName(path));
            while (parent != null)
            {
                if (parent.FullName == watcher.Path)
                {
                    break;
                }
                if ((parent.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    return true;
                }
                parent = parent.Parent;
            }
        }
        catch (Exception)
        {
        }

        return false;
    }


    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // 立てるべきフラグが立ってるなら無駄
        if (isChangeNotify)
        {
            return;
        }
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
        // 立てるべきフラグが立ってるなら無駄
        if (isChangeNotify)
        {
            return;
        }
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
