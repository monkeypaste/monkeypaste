﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace MpWinFormsUtilities
{
    public class MpLastWindowWatcher {
        public  string ThisAppPath { get; set; }
        private  IntPtr _thisAppHandle;
        private  Timer _timer;
        private  IntPtr _previousHandle;

        public MpLastWindowWatcher(IntPtr appHandle) {
            //Process.GetCurrentProcess().Invalidate();
            _thisAppHandle = appHandle;
            _previousHandle = IntPtr.Zero;
            ThisAppPath = GetProcessPath(_thisAppHandle);
            Console.WriteLine("This app's exe: " + ThisAppPath);
            _timer = new Timer(100);
            _timer.Elapsed += new ElapsedEventHandler(SetLastActive);
            _timer.Start();
        }
        public IntPtr LastHandle {
            get {
                return _previousHandle;
            }
        }
        private void SetLastActive(object sender,ElapsedEventArgs e) {
            IntPtr currentHandle = GetForegroundWindow();
            if(_thisAppHandle == IntPtr.Zero) {
                _thisAppHandle = Process.GetCurrentProcess().MainWindowHandle;
            }
            if (currentHandle != _previousHandle && currentHandle != _thisAppHandle && _thisAppHandle != IntPtr.Zero && currentHandle != IntPtr.Zero) {
                _previousHandle = currentHandle;
            }
        }
        public string GetProcessPath(IntPtr hwnd) {
            if(hwnd == IntPtr.Zero) {
                return "UNKNOWN HWND";
            }
            uint pid = 0;
            GetWindowThreadProcessId(hwnd, out pid);
            //return MpHelperSingleton.Instance.GetMainModuleFilepath((int)pid);
            Process proc = Process.GetProcessById((int)pid);
            return proc.MainModule.FileName.ToString();
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out uint processId);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
}