using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpITreeNode<T> where T : class {
        List<T> Children { get; }
    }
}
