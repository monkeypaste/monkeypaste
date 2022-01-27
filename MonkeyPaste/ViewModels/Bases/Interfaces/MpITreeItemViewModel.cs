using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpITreeItemViewModel {
        bool IsExpanded { get; set; }

        MpITreeItemViewModel ParentTreeItem { get; }

        ObservableCollection<MpITreeItemViewModel> Children { get; }
    }

    public interface MpITreeItemViewModel<T> where T:MpViewModelBase {
        bool IsExpanded { get; set; }

        T ParentTreeItem { get; }

        IList<T> Children { get; }
    }

    public static class MpITreeItemViewModelExtensions {
        public static IEnumerable<T> FindAllChildren<T>(this MpITreeItemViewModel<T> tivm) where T:MpViewModelBase {
            var activml = new List<T>();
            foreach(MpITreeItemViewModel<T> c in tivm.Children) {
                activml.Add(c as T);
                activml.AddRange(c.FindAllChildren());
            }
            return activml;
        }
    }
}
