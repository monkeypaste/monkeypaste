using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        public static IList<T> ToList<T>(this MpITreeItemViewModel<T> tivm) where T : MpViewModelBase {
            var activml = new List<T>() { tivm as T };

            activml.AddRange(tivm.FindAllChildren());
            return activml;
        }

        public static IEnumerable<T> FindAllChildren<T>(this MpITreeItemViewModel<T> tivm) where T:MpViewModelBase {
            var activml = new List<T>();
            foreach(MpITreeItemViewModel<T> c in tivm.Children) {
                activml.Add(c as T);
                activml.AddRange(c.FindAllChildren());
            }
            return activml;
        }

        public static T FindRootParent<T>(this MpITreeItemViewModel<T> tivm) where T : MpViewModelBase {
            MpITreeItemViewModel<T> rootParent = tivm.ParentTreeItem as MpITreeItemViewModel<T>;
            while (rootParent.ParentTreeItem != null) {
                rootParent = rootParent.ParentTreeItem as MpITreeItemViewModel<T>;
            }
            return rootParent as T;
        }

        public static int FindTreeLevel<T>(this MpITreeItemViewModel<T> tivm) where T : MpViewModelBase {
            int level = 0;
            MpITreeItemViewModel<T> rootParent = tivm.ParentTreeItem as MpITreeItemViewModel<T>;
            while (rootParent.ParentTreeItem != null) {
                rootParent = rootParent.ParentTreeItem as MpITreeItemViewModel<T>;
                level++;
            }
            return level;
        }
    }
}
