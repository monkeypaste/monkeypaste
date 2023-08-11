namespace MonkeyPaste.Common {
    public interface MpITempFileManager {
        void Init();
        void AddTempFilePath(string filePathToAppend);
        void RemoveLastTempFilePath();
        void Shutdown();
        void DeleteAll();
    }
}
