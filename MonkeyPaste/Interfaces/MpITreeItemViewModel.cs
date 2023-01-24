using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public interface MpITreeItemViewModel : MpIViewModel {
        bool IsExpanded { get; set; }

        MpITreeItemViewModel ParentTreeItem { get; }

        IEnumerable<MpITreeItemViewModel> Children { get; }
    }

}
