namespace MonkeyPaste {
    public interface MpIProcessIconBuilder {
        MpIIconBuilder IconBuilder { get; set; }
        string GetBase64BitmapFromFolderPath(string filepath);
        string GetBase64BitmapFromFilePath(string filepath);
    }
}
