namespace MonkeyPaste {
    public interface MpIAsyncObject {
        bool IsBusy { get; }
    }
    public interface MpIPassiveAsyncObject {
        bool IsBusy { get; set; }
    }
    public interface MpIAsyncCollectionObject {
        bool IsAnyBusy { get; }
    }
}
