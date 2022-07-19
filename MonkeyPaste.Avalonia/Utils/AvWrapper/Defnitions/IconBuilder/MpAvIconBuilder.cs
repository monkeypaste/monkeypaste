using System;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconBuilder {
        public MpIIconBuilder IconBuilder { get; private set; }

        public MpAvIconBuilder() {
            if(OperatingSystem.IsWindows()) {
                IconBuilder = new MpAvWinIconBuilder();
            } else if(OperatingSystem.IsLinux()) {
                IconBuilder = new MpAvX11IconBuilder();
            } else if(OperatingSystem.IsMacOS()) {
                IconBuilder = new MpAvMacIconBuilder();
            }
        }
    }
}

