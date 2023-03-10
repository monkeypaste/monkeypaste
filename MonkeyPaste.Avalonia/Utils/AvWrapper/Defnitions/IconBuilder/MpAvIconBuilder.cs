using System;

namespace MonkeyPaste.Avalonia {
    public class MpAvIconBuilder {
        public MpIIconBuilder IconBuilder { get; private set; }

        public MpAvIconBuilder() {
#if WINDOWS
            IconBuilder = new MpAvWinIconBuilder();
#elif LINUX
            IconBuilder = new MpAvX11IconBuilder();
#elif MAC
            IconBuilder = new MpAvMacIconBuilder();
#endif
        }
    }
}

