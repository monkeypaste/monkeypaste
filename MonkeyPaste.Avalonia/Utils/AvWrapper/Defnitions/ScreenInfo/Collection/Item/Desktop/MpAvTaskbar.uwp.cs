﻿using MonkeyPaste.Common;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;

namespace MonkeyPaste.Avalonia {
    public class MpAvTaskbarViewModel : MpAvViewModelBase {
        private const string ClassName = "Shell_TrayWnd";

        private static MpAvTaskbarViewModel _instance;
        public static MpAvTaskbarViewModel Instance => _instance ?? (_instance = new MpAvTaskbarViewModel());

        public MpRect Bounds { get; private set; }
        public TaskbarPosition Position { get; private set; }

        //Always returns false under Windows 7
        public bool AlwaysOnTop { get; private set; }
        public bool AutoHide { get; private set; }

        public bool IsVisible { get; private set; }

        public MpAvTaskbarViewModel() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            Init();
        }


        public void Init() {
            IntPtr taskbarHandle = User32.FindWindow(MpAvTaskbarViewModel.ClassName, null);

            APPBARDATA data = new APPBARDATA();
            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            data.hWnd = taskbarHandle;
            IntPtr result = Shell32.SHAppBarMessage(ABM.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero)
                throw new InvalidOperationException();

            this.Position = (TaskbarPosition)data.uEdge;
            Rectangle rect = Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);
            var new_bounds = new MpRect(rect.X, rect.Y, rect.Width, rect.Height);
            if (!Bounds.IsEqual(new_bounds)) {
                // careful w/ rect property
                Bounds = new_bounds;
            }

            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            result = Shell32.SHAppBarMessage(ABM.GetState, ref data);
            int state = result.ToInt32();
            this.AlwaysOnTop = (state & ABS.AlwaysOnTop) == ABS.AlwaysOnTop;
            this.AutoHide = (state & ABS.Autohide) == ABS.Autohide;
            this.IsVisible = IsTaskbarVisible();

            //MpConsole.WriteLine(this.ToString());
        }
        public override string ToString() {
            return $"TaskBar Position: '{Position}' Bounds: '{Bounds}' AlwaysOnTop: {AlwaysOnTop} AutoHide: {AutoHide} IsVisible: {IsVisible}";
        }

        private bool IsTaskbarVisible() {
            return Math.Abs(SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height) > 0;
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowOpening:
                    Init();
                    break;
            }
        }
    }


    public enum TaskbarPosition {
        Unknown = -1,
        Left,
        Top,
        Right,
        Bottom,
    }

    public enum ABM : uint {
        New = 0x00000000,
        Remove = 0x00000001,
        QueryPos = 0x00000002,
        SetPos = 0x00000003,
        GetState = 0x00000004,
        GetTaskbarPos = 0x00000005,
        Activate = 0x00000006,
        GetAutoHideBar = 0x00000007,
        SetAutoHideBar = 0x00000008,
        WindowPosChanged = 0x00000009,
        SetState = 0x0000000A,
    }

    public enum ABE : uint {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    public static class ABS {
        public const int Autohide = 0x0000001;
        public const int AlwaysOnTop = 0x0000002;
    }

    public static class Shell32 {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);
    }

    public static class User32 {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public ABE uEdge;
        public RECT rc;
        public int lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
