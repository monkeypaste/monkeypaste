namespace MonkeyPaste.Common {
    public interface MpIDebugBreakHelper {
        void HandlePreBreak();
        void HandlePostBreak();
        void ToggleBreak();
    }
}
