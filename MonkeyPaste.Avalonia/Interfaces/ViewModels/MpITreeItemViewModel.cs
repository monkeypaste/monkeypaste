using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {

    public interface MpITreeItemViewModel : MpIExpandableViewModel {

        MpITreeItemViewModel ParentTreeItem { get; }

        IEnumerable<MpITreeItemViewModel> Children { get; }
    }

}
