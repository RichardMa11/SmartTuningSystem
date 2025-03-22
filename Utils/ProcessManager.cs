﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace SmartTuningSystem.Utils
{
    internal static class ProcessManager
    {
        private static Mutex _processLock;
        private static bool _hasLock;

        /// <summary>
        /// 获取进程锁
        /// </summary>
        public static void GetProcessLock()
        {
            // 全局锁，锁名称可以自定义。
            _processLock = new Mutex(false, $"Global\\SmartTuningSystem[{GetUid()}]", out _hasLock);

            if (!_hasLock)
            {
                ActiveWindow();
                Environment.Exit(0);
            }
        }

        private static string GetUid()
        {
            var bytes = Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().Location);
            using (var md5 = MD5.Create())
            {
                bytes = md5.ComputeHash(bytes);
            }
            return BitConverter.ToString(bytes);
        }

        /// <summary>
        /// 激活当前进程并将其窗口放到屏幕最前面
        /// </summary>
        public static void ActiveWindow()
        {
            using (var p = Process.GetCurrentProcess())
            {
                string pName = p.ProcessName;
                Process[] temp = Process.GetProcessesByName(pName);
                foreach (var item in temp)
                {
                    if (item.MainModule.FileName == p.MainModule.FileName)
                    {
                        IntPtr handle = item.MainWindowHandle;
                        SwitchToThisWindow(handle, true);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 释放当前进程的锁
        /// </summary>
        /// <remarks>小心使用</remarks>
        public static void ReleaseLock()
        {
            if (_processLock != null && _hasLock)
            {
                _processLock.Dispose();
                _hasLock = false;
            }
        }

        // 将另一个窗口激活放到前台。
        // Win32 API
        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);
    }
}
