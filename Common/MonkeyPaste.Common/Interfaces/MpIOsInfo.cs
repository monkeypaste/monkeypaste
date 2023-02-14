namespace MonkeyPaste.Common {
    public interface MpIOsInfo {
        string OsMachineName { get; }
        string OsFileManagerPath { get; }
        string OsFileManagerName { get; }
        bool IsAvalonia { get; }

        MpUserDeviceType OsType { get; }
    }
}
