using System;
#if WINDOWS

using MonkeyPaste.Common.Wpf;
using static MonkeyPaste.Common.Wpf.WinApi;
#endif
namespace MonkeyPaste.Avalonia {
    public static class MpAvToolWindow_Win32 {
        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        //[DllImport("user32.dll")]
        //public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        //[DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        //public static extern void SetLastError(int dwErrorCode);
        //[DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        //public static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        //[DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        //public static extern int IntSetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public static void InitToolWindow(IntPtr handle) {
#if WINDOWS
            int cur_style = GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            cur_style |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)cur_style);
#endif

        }

        public static void SetAsNoHitTestWindow(IntPtr handle) {
            // from https://github.com/AvaloniaUI/Avalonia/issues/4956
            // see thread for other platforms
#if WINDOWS
            int cur_style = GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, cur_style | (int)ExtendedWindowStyles.WS_EX_LAYERED | (int)ExtendedWindowStyles.WS_EX_TRANSPARENT); 
#endif
            //SetLayeredWindowAttributes(handle, 0, 255, 0x2);
        }

        public static void RemoveNoHitTestWindow(IntPtr handle) {
#if WINDOWS
            int cur_style = GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            int with_hit_test_style = cur_style & (~(int)ExtendedWindowStyles.WS_EX_LAYERED) & (~(int)ExtendedWindowStyles.WS_EX_TRANSPARENT);
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, with_hit_test_style); 
#endif
        }

    }

}
