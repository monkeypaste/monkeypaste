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
            int exStyle = WinApi.GetWindowLong(handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)WinApi.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            WinApi.SetWindowLong(handle, (int)WinApi.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
#endif

        }
    }

}
