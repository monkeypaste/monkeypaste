using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Wpf;

namespace AvCoreClipboardHandler {
    public static class Util {

        public static async Task WaitForClipboard() {
            if (OperatingSystem.IsWindows()) {
                bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(50);
                    canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                }
            }
        }

        public static void CloseClipboard() {
            if (OperatingSystem.IsWindows()) {
                WinApi.CloseClipboard();
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
