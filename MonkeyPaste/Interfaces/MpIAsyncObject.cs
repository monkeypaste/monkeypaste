namespace MonkeyPaste {
    public interface MpIAsyncObject {
        bool IsBusy { get; }
    }
    public interface MpIAsyncCollectionObject {
        bool IsAnyBusy { get; }
    }
}
