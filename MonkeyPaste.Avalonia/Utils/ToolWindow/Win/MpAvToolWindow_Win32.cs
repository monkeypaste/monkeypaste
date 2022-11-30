using System.Runtime.InteropServices;
using System;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Wpf;
using MonkeyPaste.Common;
using Avalonia;

namespace MonkeyPaste.Avalonia {
    public static class MpAvToolWindow_Win32 {
        [Flags]
        public enum ExtendedWindowStyles {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }
        public enum GetWindowLongFields {
            // ...
            GWL_EXSTYLE = -20,
            // ...
        }
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        public static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        public static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void InitToolWindow(IntPtr handle) {
            int exStyle = WinApi.GetWindowLong(handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

        }
    }

    public static class MpAvTopMostWindow {
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SWP_NOMOVE = 0x0002;
        enum WindowLongFlags : int {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }

        private const uint WS_EX_TOPMOST = 0x00000008;



        public static void InitTopmostWindow(IntPtr handle, int x, int y, int cx, int cy) {
            WinApi.SetWindowPos(handle, HWND_TOPMOST, x,y,cx,cy, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
        }
    }
}
