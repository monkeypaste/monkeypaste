//using Avalonia.Win32;

namespace MonkeyPaste {
    public interface MpIShutdownTools {
        void ShutdownApp(MpShutdownType code, string detail);
    }
}
