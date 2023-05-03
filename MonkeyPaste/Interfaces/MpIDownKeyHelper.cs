namespace MonkeyPaste {
    public interface MpIDownKeyHelper {
        int DownCount { get; }
        bool IsDown(object key);
    }
}
