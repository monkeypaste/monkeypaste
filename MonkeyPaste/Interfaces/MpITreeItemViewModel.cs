using System.Collections.Generic;

namespace MonkeyPaste {

    public interface MpITreeItemViewModel : MpIExpandableViewModel {

        MpITreeItemViewModel ParentTreeItem { get; }

        IEnumerable<MpITreeItemViewModel> Children { get; }
    }

}
