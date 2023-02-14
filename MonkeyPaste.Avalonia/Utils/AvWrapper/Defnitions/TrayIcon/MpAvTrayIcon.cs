using System;

namespace MonkeyPaste.Avalonia {

    public class MpAvTrayIcon {
        public MpITrayIcon TrayIcon { get; }

        public MpAvTrayIcon() {
            if (OperatingSystem.IsMacOS()) {
                TrayIcon = new MpAvMacTrayIcon();
            } else if (OperatingSystem.IsWindows()) {

            }
        }
    }
}

