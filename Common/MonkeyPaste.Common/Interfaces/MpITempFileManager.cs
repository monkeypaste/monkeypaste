namespace MonkeyPaste.Common {
    public interface MpITempFileManager {
        void Init();
        void AddTempFilePath(string filePathToAppend);
        void Shutdown();
        void DeleteAll();
    }
}
