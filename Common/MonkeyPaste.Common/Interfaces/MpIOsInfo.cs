namespace MonkeyPaste.Common {
    public interface MpIOsInfo {
        string OsMachineName { get; }
        string OsFileManagerPath { get; }
        string OsFileManagerName { get; }
        bool IsDesktop { get; }
        string OsShortName { get; }

        MpUserDeviceType OsType { get; }
    }
}
