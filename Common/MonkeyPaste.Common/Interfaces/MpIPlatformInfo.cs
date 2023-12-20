using System.Diagnostics;

namespace MonkeyPaste.Common {

    public interface MpIPlatformInfo {
        string OsMachineName { get; }
        string OsVersionInfo { get; }
        string OsFileManagerPath { get; }

        string ExecutableName { get; }
        string ExecutingDir { get; }
        string ExecutingPath { get; }
        string StorageDir { get; }
        string LogDir { get; }
        string LogPath { get; }
        bool IsDesktop { get; }
        bool IsMobile { get; }
        bool IsBrowser { get; }
        bool IsTouchInputEnabled { get; }
        bool IsTraceEnabled { get; }
        string OsShortName { get; }

        string EditorPath { get; }
        string TermsPath { get; }

        TraceListener ConsoleTraceListener { get; }

        MpUserDeviceType OsType { get; }
    }
}
