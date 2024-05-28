using System;
using MonkeyPaste.Common;

#if WINDOWS

using MonkeyPaste.Common.Wpf;
using static MonkeyPaste.Common.Wpf.WinApi;
#endif
namespace MonkeyPaste.Avalonia {
    public static class MpAvToolWindow_Win32 {

        public static void SetAsToolWindow(IntPtr handle) {
#if WINDOWS && !WINDOWED
            int cur_style_val = GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            cur_style_val |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)cur_style_val);
#endif
        }

        public static void UnsetAsToolWindow(IntPtr handle) {
#if WINDOWS && !WINDOWED
            GetWindowLongFields cur_style = (GetWindowLongFields)GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            MpDebug.Assert(IsToolWindow(handle), "Warning, not tool window");
            cur_style.RemoveFlag(GetWindowLongFields.GWL_EXSTYLE);
            MpDebug.Assert(!cur_style.HasFlag(GetWindowLongFields.GWL_EXSTYLE), "Error, flag remove didn't work");
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, (int)cur_style);
            MpDebug.Assert(!IsToolWindow(handle), "Error, pinvoke didn't work");
#endif
        }

        public static bool IsToolWindow(IntPtr handle) {
#if WINDOWS && !WINDOWED
            GetWindowLongFields cur_style = (GetWindowLongFields)GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            return cur_style.HasFlag(GetWindowLongFields.GWL_EXSTYLE);
#else
            return false;
#endif
        }

        public static void SetAsNoHitTestWindow(IntPtr handle) {
            // from https://github.com/AvaloniaUI/Avalonia/issues/4956
            // see thread for other platforms
#if WINDOWS && !WINDOWED
            int cur_style_val = GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, cur_style_val | (int)ExtendedWindowStyles.WS_EX_LAYERED | (int)ExtendedWindowStyles.WS_EX_TRANSPARENT);
#endif
            //SetLayeredWindowAttributes(handle, 0, 255, 0x2);
        }

        public static void RemoveNoHitTestWindow(IntPtr handle) {
#if WINDOWS && !WINDOWED
            int cur_style_val = GetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE);
            int with_hit_test_style_val = cur_style_val & (~(int)ExtendedWindowStyles.WS_EX_LAYERED) & (~(int)ExtendedWindowStyles.WS_EX_TRANSPARENT);
            SetWindowLong(handle, (int)GetWindowLongFields.GWL_EXSTYLE, with_hit_test_style_val);
#endif
        }

    }

}
