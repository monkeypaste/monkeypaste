namespace MonkeyPaste {
    public interface MpILoadOnLoginTools {
        bool IsLoadOnLoginEnabled { get; }
        void SetLoadOnLogin(bool isLoadOnLogin);
    }
}
