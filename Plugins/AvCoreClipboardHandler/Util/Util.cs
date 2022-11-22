using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvCoreClipboardHandler {
    public static class Util {

        public static async Task WaitForClipboard() {
            if (OperatingSystem.IsWindows()) {
                bool canOpen = MonkeyPaste.Common.Avalonia.WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(50);
                    canOpen = MonkeyPaste.Common.Avalonia.WinApi.IsClipboardOpen() == IntPtr.Zero;
                }
            }
        }

        public static void CloseClipboard() {
            if (OperatingSystem.IsWindows()) {
                MonkeyPaste.Common.Avalonia.WinApi.CloseClipboard();
            }
        }

        public static MpPluginUserNotificationFormat CreateNotification(
            MpPluginNotificationType nftype,
            string title,
            string msg,
            string detail) {
            return new MpPluginUserNotificationFormat() {
                NotificationType = nftype,
                Title = title,
                Body = msg,
                Detail = detail,
                IconSourceStr = MpBase64Images.ClipboardIcon
            };
        }
    }
}
