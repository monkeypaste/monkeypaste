﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using HWND = System.IntPtr;

namespace MonkeyPaste.UWP {
    public static class MpOpenWindowGetter {
        public static IDictionary<IntPtr, string> GetOpenWindows() {
            IntPtr shellWindow = WinApi.GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            WinApi.EnumWindows(delegate (IntPtr hWnd, int lParam) {
                if (hWnd == shellWindow) {
                    return true;
                }
                if (!WinApi.IsWindowVisible(hWnd)) {
                    return true;
                }

                try {
                    WinApi.GetWindowThreadProcessId(hWnd, out uint pid);
                    var process = Process.GetProcessById((int)pid);
                    if (process.MainWindowHandle == IntPtr.Zero) {
                        return true;
                    }

                    int length = WinApi.GetWindowTextLength(hWnd);
                    if (length == 0) return true;

                    //if(MpHelpers.Instance.IsThisAppAdmin()) {
                    //    process.WaitForInputIdle(100);
                    //}
                    
                    StringBuilder builder = new StringBuilder(length);
                    WinApi.GetWindowText(hWnd, builder, length + 1);

                    windows[hWnd] = builder.ToString();
                }
                catch (InvalidOperationException ex) {
                    // no graphical interface
                    MonkeyPaste.MpConsole.WriteLine("OpenWindowGetter, ignoring non GUI window w/ error: " + ex.ToString());
                }

                return true;

            }, 0);

            return windows;
        }

    }
}