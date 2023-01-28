using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIContentQueryTools {
        IEnumerable<int> GetOmittedContentIds();
    }

    public interface MpITagQueryTools {
        IEnumerable<int> GetSelfAndAllAncestorTagIds(int tagId);
        IEnumerable<int> GetSelfAndAllDescendantsTagIds(int tagId);
    }
}
