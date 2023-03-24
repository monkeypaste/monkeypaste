using System;

namespace MonkeyPaste.Avalonia {

    public class MpAvTrayIcon {
        public MpITrayIcon TrayIcon { get; }

        public MpAvTrayIcon() {
#if MAC
         TrayIcon = new MpAvMacTrayIcon();
#endif
        }
    }
}

