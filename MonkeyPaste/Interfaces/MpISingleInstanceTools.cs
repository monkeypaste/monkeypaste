namespace MonkeyPaste {
    public interface MpISingleInstanceTools {
        bool IsFirstInstance { get; }
        bool DoInstanceCheck();
        bool RemoveInstanceLock();
    }
}
