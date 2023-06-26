using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIContentQueryPage {
        IEnumerable<int> GetOmittedContentIds();
        IEnumerable<int> GetPlaceholderContentIds();
        int Offset { get; }
        int Limit { get; }
        IEnumerable<int> ContentIds { get; }
    }

    public interface MpITagQueryTools {
        IEnumerable<int> GetSelfAndAllAncestorTagIds(int tagId);
        IEnumerable<int> GetSelfAndAllDescendantsTagIds(int tagId);
    }
}
