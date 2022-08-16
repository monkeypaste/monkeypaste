namespace MonkeyPaste {
    public interface MpIOsInfo {
        string OsMachineName { get; }
        string OsFileManagerPath { get; }
        string OsFileManagerName { get; }

        MpUserDeviceType OsType { get; }
    }
}
