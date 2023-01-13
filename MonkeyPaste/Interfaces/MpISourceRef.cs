using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste {
    public interface MpISourceRef : MpIIconResource, MpILabelText {
        int Priority { get; }
        int SourceObjId { get; }
        MpCopyItemSourceType SourceType { get; }
    }
}
