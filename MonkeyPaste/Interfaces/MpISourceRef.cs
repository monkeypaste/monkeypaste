namespace MonkeyPaste {
    public interface MpISourceRef {
        int Priority { get; }
        int SourceObjId { get; }
        MpCopyItemSourceType SourceType { get; }
    }
}
