using System;

namespace MonkeyPaste.Avalonia {

    public class MpAvTrayIcon {
        public MpAvITrayIcon TrayIcon { get; }

        public MpAvTrayIcon() {
#if MAC
         TrayIcon = new MpAvMacTrayIcon();
#endif
        }
    }
}

