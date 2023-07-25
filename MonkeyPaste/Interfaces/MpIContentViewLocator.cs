namespace MonkeyPaste {
    public interface MpIContentViewLocator {
        MpIContentView LocateContentView(int contentId);
        void AddView(MpIContentView cv);
        void RemoveView(MpIContentView cv);
    }
}
