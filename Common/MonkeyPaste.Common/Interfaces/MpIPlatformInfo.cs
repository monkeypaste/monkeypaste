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
        bool IsDesktop { get; }
        string OsShortName { get; }

        MpUserDeviceType OsType { get; }
    }
}
