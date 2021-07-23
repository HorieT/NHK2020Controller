using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ABU2021_ControlAndDebug
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// 
    /// 
    /// 多重起動の防止はここ
    /// https://aibax.hatenadiary.org/entry/20080711/1215787534
    /// </summary>
    public partial class App : Application
    {
        /*
         * Win32APIをP/Invokeで使用するための宣言
         */
        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_HIDE           = 0;
        private const int SW_SHOWNORMAL     = 1;
        private const int SW_SHOWMINIMIZED  = 2;
        private const int SW_SHOWMAXIMIZED  = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOW           = 5;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /* 現在のプロセスを取得 */
            Process currentProcess = Process.GetCurrentProcess();

            /* 現在のプロセスと同名のプロセスを取得 */
            Process[] processes = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (Process p in processes)
            {
                /* 同名の他のプロセスがあれば... */
                if (p.Id != currentProcess.Id)
                {
                    /* ウィンドウを全面に表示する */
                    ShowWindow(p.MainWindowHandle, SW_SHOWMAXIMIZED);
                    SetForegroundWindow(p.MainWindowHandle);

                    /* 起動を中止してプログラムを終了 */
                    this.Shutdown();
                }
            }
        }
    }
}
