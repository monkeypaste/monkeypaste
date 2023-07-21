//using Avalonia.Win32;

namespace MonkeyPaste {
    public interface MpIShutdownTools {
        bool WasShutdownSignaled { get; }
        void ShutdownApp(object args);
    }
}
