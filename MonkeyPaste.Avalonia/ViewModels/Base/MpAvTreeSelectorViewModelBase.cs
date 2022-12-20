using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvTreeSelectorViewModelBase<P, C> :
        MpAvSelectorViewModelBase<P, C>, MpITreeItemViewModel
        where P : class
        where C : MpViewModelBase, MpISelectableViewModel, MpITreeItemViewModel {

        #region MpITreeItemViewModel Implementation

        public virtual bool IsExpanded { get; set; }
        public abstract MpITreeItemViewModel ParentTreeItem { get; }
        public virtual IEnumerable<MpITreeItemViewModel> Children => Items;

        #endregion

        #region Properties

        #region View Models
        public virtual IEnumerable<MpITreeItemViewModel> AllAncestors {
            get {
                var aal = new List<MpITreeItemViewModel>();
                var cur = (MpITreeItemViewModel)this;
                while (cur != null) {
                    if (cur.ParentTreeItem != null) {
                        aal.Add(cur.ParentTreeItem);
                    }
                    cur = cur.ParentTreeItem;
                }
                return aal;
            }
        }

        public virtual IEnumerable<MpITreeItemViewModel> AllDescendants {
            get {
                var adl = new List<MpITreeItemViewModel>();
                foreach (var cttvm in Items) {
                    adl.Add(cttvm);
                    if(cttvm is MpAvTreeSelectorViewModelBase<P,C> tree_cttvm) {
                        adl.AddRange(tree_cttvm.AllDescendants);
                    }
                }
                return adl;
            }
        }

        public virtual IEnumerable<MpITreeItemViewModel> SelfAndAllDescendants {
            get {
                var saldttvml = AllDescendants.ToList();
                saldttvml.Insert(0, this);
                return saldttvml;
            }
        }

        public virtual IEnumerable<MpITreeItemViewModel> SelfAndAllAncestors {
            get {
                var salattvml = AllAncestors.ToList();
                salattvml.Insert(0, this);
                return salattvml;
            }
        }

        public virtual MpITreeItemViewModel RootItem => this.FindRootParent();

        #endregion

        #endregion

        public MpAvTreeSelectorViewModelBase() : base(null) { }

        public MpAvTreeSelectorViewModelBase(P p) : base(p) { }

    }
}
