using MonkeyPaste.Common.Plugin;
using System.Diagnostics;

namespace MonkeyPaste.Common {

    public interface MpIPlatformInfo {
        string OsMachineName { get; }
        string OsVersion { get; }
        string OsFileManagerPath { get; }
        string RuntimeShortName { get; }

        string ExecutableName { get; }
        string ExecutingDir { get; }
        string ExecutingPath { get; }
        string StorageDir { get; }
        string LogDir { get; }
        string LogPath { get; }
        string LoggingEnabledCheckPath { get; }
        bool IsDesktop { get; }
        bool IsMobile { get; }
        bool IsBrowser { get; }
        bool IsTouchInputEnabled { get; }
        bool IsTraceEnabled { get; }
        string OsShortName { get; }

        string EditorPath { get; }
        string TermsPath { get; }
        string CreditsPath { get; }

        TraceListener ConsoleTraceListener { get; }

        MpUserDeviceType OsType { get; }
    }
}
