using HmNetCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HmGitWatcher;


public partial class HmGitWatcher
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetDpiForWindow(IntPtr hWnd);

    static private int lastDpi = 96;

    public int GetHidemaruDpi()
    {
        int curDPI = GetDpi(); // XとYは同じ値になるはずです
        if (curDPI > 0)
        {
            lastDpi = curDPI;
        }
        return curDPI;
    }


    static public int GetDpi()
    {
        var hWnd = Hm.WindowHandle;

        if ((IntPtr)hWnd == IntPtr.Zero)
        {
            return 0;
        }

        int dpi = GetDpiForWindow((IntPtr)hWnd);
        if (dpi == 0)
        {
            return 0;
        }

        return dpi;
    }




    DpiWatcher dpiWatcher;
    public void ReCreateDPIWatcher(dynamic func)
    {
        if (dpiWatcher != null)
        {
            dpiWatcher.Stop();
        }

        dpiWatcher = new DpiWatcher(func);
    }

    public void StopDPIWatcher()
    {
        if (dpiWatcher != null)
        {
            dpiWatcher.Stop();
        }
    }

    internal class DpiWatcher
    {
        private dynamic callBackFunc;
        public DpiWatcher(dynamic func)
        {
            this.callBackFunc = func;
            this.Start();
        }

        // デストラクタ
        ~DpiWatcher()
        {
            Hm.OutputPane.Output("DpiWatcher デストラクタ\r\n");
            this.Stop();
        }

        private CancellationTokenSource _cancellationTokenSource;
        private void StartCheck(dynamic callBackFoundRepos, dynamic callBackStatusChange)
        {


        }
        public void Start()
        {
            this.Stop();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            // 非同期処理を開始
            Task.Run(async () => await CheckDPI(callBackFunc, cancellationToken));
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
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async Task CheckDPI(dynamic func, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {

                    // 1秒間隔
                    for (int i = 0; i < 4; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                        await Task.Delay(250, cancellationToken);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }


                    var curDPI = GetDpi();

                    if (curDPI != lastDpi)
                    {
                        lastDpi = curDPI;

                        try
                        {
                            func(curDPI);
                        }
                        catch (Exception ex)
                        {
                            this.Stop();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }

}