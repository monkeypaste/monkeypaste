﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MpWinFormsApp {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public readonly int Left;
        public readonly int Top;
        public readonly int Right;
        public readonly int Bottom;

        private RECT(int left,int top,int right,int bottom) {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public RECT(Rectangle r) : this(r.Left,r.Top,r.Right,r.Bottom) {
        }
    }
    /// <summary>
    /// A wrapper for various WinAPI functions.
    /// </summary>
    /// <remarks>
    /// This class is just a wrapper for your various WinApi functions.
    /// In this sample only the bare essentials are included.
    /// In my own WinApi class, I have all the WinApi functions that any
    /// of my applications would ever need.
    /// 
    /// From http://www.codeproject.com/KB/cs/SingleInstanceAppMutex.aspx
    /// </remarks>
    public static class WinApi {
        [StructLayout(LayoutKind.Sequential)]
        public struct PointInter {
            public int X;
            public int Y;
            public static explicit operator Point(PointInter point) => new Point(point.X, point.Y);
        }

        public const int WM_CLIPBOARDUPDATE = 0x031D;
        public const int EM_SETRECT = 0xB3;
        public const int HWND_BROADCAST = 0xffff;
        public const int SW_SHOWNORMAL = 1;
        
        [DllImport("user32.dll")]
        public  static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
        internal static int RegisterWindowMessage(string format, params object[] args)
        {
            string message = String.Format(format, args);
            return RegisterWindowMessage(message);
        }
        
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImportAttribute("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        internal static void ShowToFront(IntPtr window)
        {
            ShowWindow(window, SW_SHOWNORMAL);
            SetForegroundWindow(window);
        }
        [DllImport("User32.dll")]
        public static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll",CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove,IntPtr hWndNewNext);

        [DllImport("user32.dll",CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd,int wMsg,IntPtr wParam,IntPtr lParam);

        /// <summary>
        /// Places the given window in the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll",SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AddClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Removes the given window from the system-maintained clipboard format listener list.
        /// </summary>
        [DllImport("user32.dll",SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        /// <summary>
        /// Sent when the contents of the clipboard have changed.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool SetActiveWindow(IntPtr hWnd);

        // Registers a hot key with Windows.
        //[DllImport("user32.dll")]
        //public static extern bool RegisterHotKey(IntPtr hWnd,int id,uint fsModifiers,uint vk);

        //// Unregisters the hot key with Windows.
        //[DllImport("user32.dll")]
        //public static extern bool UnregisterHotKey(IntPtr hWnd,int id);

        [DllImport(@"User32.dll",EntryPoint = @"SendMessage",CharSet = CharSet.Auto)]
        public static extern int SendMessageRefRect(IntPtr hWnd,uint msg,int wParam,ref RECT rect);

        [DllImport("User32.dll")]
        public static extern bool SetProcessDPIAware();

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc,int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd,IntPtr hDC);


        [DllImport("user32.dll",CharSet = CharSet.Auto,SetLastError = true)]
        public static extern int GetWindowThreadProcessId(IntPtr handle,out uint processId);

        [DllImport("user32.dll",EntryPoint = "FindWindow",SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr zeroOnly,string lpWindowName);

        [DllImport("user32",SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hwnd,int id,uint fsModifiers,uint vk);
        [DllImport("user32",SetLastError = true)]
        public static extern int UnregisterHotKey(IntPtr hwnd,int id);
        [DllImport("kernel32",SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);
        [DllImport("kernel32",SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);

        [DllImport("user32.dll")]
        public static extern short VkKeyScan(char ch);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern short VkKeyScanEx(char ch, IntPtr dwhkl);
        [DllImport("user32.dll")]
        public static extern bool UnloadKeyboardLayout(IntPtr hkl);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);


        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out PointInter lpPoint);
    }
}