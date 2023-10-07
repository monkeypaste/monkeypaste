namespace MonkeyPaste.Common {

    public interface MpIPlatformInfo {
        string OsMachineName { get; }
        string OsVersionInfo { get; }
        string OsFileManagerPath { get; }
        string OsFileManagerName { get; }

        string ExecutableName { get; }
        string ExecutingDir { get; }
        string ExecutingPath { get; }
        string StorageDir { get; }
        string LogDir { get; }
        bool IsDesktop { get; }
        bool IsMobile { get; }
        bool IsBrowser { get; }
        bool IsTouchInputEnabled { get; }
        string OsShortName { get; }

        string EditorPath { get; }
        string TermsPath { get; }

        MpUserDeviceType OsType { get; }
    }
}
