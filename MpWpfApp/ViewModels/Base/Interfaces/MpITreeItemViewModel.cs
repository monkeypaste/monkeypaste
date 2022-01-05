using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public interface MpITreeItemViewModel {
        bool IsSelected { get; set; }
        bool IsHovering { get; set; }
        bool IsExpanded { get; set; }

        MpITreeItemViewModel ParentTreeItem { get; }

        ObservableCollection<MpITreeItemViewModel> Children { get; }
    }
}
