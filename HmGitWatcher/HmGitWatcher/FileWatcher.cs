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
    private bool isFileChanged = false;

    public void CreateFileWatcher(string targetPath)
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

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (e?.FullPath?.Contains(".git") == true)
        {
            return;
        }

        Hm.OutputPane.Output(e?.FullPath + "\r\n");
        // イベントが発生した場合は変更フラグを立てる
        isFileChanged = true;
        Hm.OutputPane.Output("OnFileChanged" + "\r\n");
    }

    private void OnFileChanged(object sender, RenamedEventArgs e)
    {
        if (e?.FullPath?.Contains(".git") == true)
        {
            return;
        }

        Hm.OutputPane.Output(e?.FullPath + "\r\n");
        // イベントが発生した場合は変更フラグを立てる
        isFileChanged = true;
        Hm.OutputPane.Output("OnFileChanged" + "\r\n");
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
