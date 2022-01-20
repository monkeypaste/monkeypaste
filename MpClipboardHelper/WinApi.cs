using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MpClipboardHelper {
    public static class WinApi {
        public const int WM_DRAWCLIPBOARD = 0x308;
        public const int WM_CHANGECBCHAIN = 0x030D;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool SetActiveWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern IntPtr GetOpenClipboardWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowText(int hwnd, StringBuilder text, int count);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowTextLength(int hwnd);
    }
}
