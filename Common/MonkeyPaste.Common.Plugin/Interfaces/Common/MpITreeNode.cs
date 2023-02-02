using System.Collections.Generic;

namespace MonkeyPaste.Common.Plugin {
    public interface MpITreeNode : MpIExpandableViewModel {
        IEnumerable<MpITreeNode> Children { get; }
    }
}
