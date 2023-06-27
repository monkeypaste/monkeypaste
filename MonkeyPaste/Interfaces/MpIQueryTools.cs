using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIContentQueryPage {
        IEnumerable<int> GetOmittedContentIds();
        int Offset { get; }
        int Limit { get; }
    }

    public interface MpITagQueryTools {
        IEnumerable<int> GetSelfAndAllAncestorTagIds(int tagId);
        IEnumerable<int> GetSelfAndAllDescendantsTagIds(int tagId);
    }
}
