using System;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconBuilder {
        public MpIIconBuilder IconBuilder { get; private set; }

        public MpAvIconBuilder() {
            if(OperatingSystem.IsWindows()) {

            } else if(OperatingSystem.IsLinux()) {

            } else if(OperatingSystem.IsMacOS()) {
                IconBuilder = new MpAvMacIconBuilder();
            }
        }
    }
}

