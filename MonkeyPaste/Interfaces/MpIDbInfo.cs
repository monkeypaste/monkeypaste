namespace MonkeyPaste {
    public interface MpIDbInfo {
        string DbExtension { get; }
        string DbFileName { get; }
        string DbDir { get; }
        string DbPath { get; }
        string DbPassword { get; }
    }

}
