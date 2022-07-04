namespace MonkeyPaste {
    public interface MpIOsInfo {
        string OsFileManagerPath { get; }
        string OsFileManagerName { get; }

        MpUserDeviceType OsType { get; }
    }
}
