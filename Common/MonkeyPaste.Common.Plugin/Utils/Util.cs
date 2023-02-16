namespace MonkeyPaste.Common.Plugin {
    public static class Util {

        //public static async Task WaitForClipboard() {
        //    if (OperatingSystem.IsWindows()) {
        //        bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
        //        while (!canOpen) {
        //            await Task.Delay(50);
        //            canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
        //        }
        //    }
        //}

        //public static void CloseClipboard() {
        //    if (OperatingSystem.IsWindows()) {
        //        WinApi.CloseClipboard();
        //    }
        //}

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
                IconSourceObj = MpBase64Images.ClipboardIcon
            };
        }
    }
}
