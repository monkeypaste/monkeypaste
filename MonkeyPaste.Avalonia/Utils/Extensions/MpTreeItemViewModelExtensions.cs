using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpTreeItemViewModelExtensions {

        public static IEnumerable<MpITreeItemViewModel> AllDescendants(this MpITreeItemViewModel tivm) {
            var activml = new List<MpITreeItemViewModel>();
            if (tivm == null || tivm.Children == null) {
                return activml;
            }
            foreach (MpITreeItemViewModel c in tivm.Children) {
                activml.Add(c);
                var ccl = c.AllDescendants();
                foreach (var cc in ccl) {
                    activml.Add(cc);
                }
            }
            return activml;
        }

        public static MpITreeItemViewModel RootParent(this MpITreeItemViewModel tivm) {
            if (tivm == null || tivm.ParentTreeItem == null) {
                return tivm;
            }
            return tivm.ParentTreeItem.RootParent();
        }

        public static IEnumerable<MpITreeItemViewModel> AllAncestors(this MpITreeItemViewModel tivm) {
            var aal = new List<MpITreeItemViewModel>();
            var cur = tivm;
            while (cur != null) {
                if (cur.ParentTreeItem != null) {
                    aal.Add(cur.ParentTreeItem);
                }
                cur = cur.ParentTreeItem;
            }
            return aal;
        }


        public static IEnumerable<MpITreeItemViewModel> SelfAndAllDescendants(this MpITreeItemViewModel tivm) {
            var saldttvml = tivm.AllDescendants().ToList();
            saldttvml.Insert(0, tivm);
            return saldttvml;
        }

        public static IEnumerable<MpITreeItemViewModel> SelfAndAllAncestors(this MpITreeItemViewModel tivm) {
            var salattvml = tivm.AllAncestors().ToList();
            salattvml.Insert(0, tivm);
            return salattvml;
        }
    }
}
