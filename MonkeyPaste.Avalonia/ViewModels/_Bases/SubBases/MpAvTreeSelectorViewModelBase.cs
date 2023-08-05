using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvTreeSelectorViewModelBase<P, C> :
        MpAvSelectorViewModelBase<P, C>, MpITreeItemViewModel
        where P : class
        where C : MpAvViewModelBase, MpISelectableViewModel, MpITreeItemViewModel {

        #region MpITreeItemViewModel Implementation

        public virtual bool IsExpanded { get; set; }
        public abstract MpITreeItemViewModel ParentTreeItem { get; }
        public virtual IEnumerable<MpITreeItemViewModel> Children => Items;

        #endregion

        #region Properties

        #region View Models
        public virtual IEnumerable<MpITreeItemViewModel> AllAncestors =>
            this.AllAncestors();

        public virtual IEnumerable<MpITreeItemViewModel> AllDescendants =>
            this.AllDescendants();

        public virtual IEnumerable<MpITreeItemViewModel> SelfAndAllDescendants =>
            this.SelfAndAllDescendants();

        public virtual IEnumerable<MpITreeItemViewModel> SelfAndAllAncestors =>
            this.SelfAndAllAncestors();


        public virtual MpITreeItemViewModel RootItem =>
            this.RootParent();

        #endregion

        #endregion

        public MpAvTreeSelectorViewModelBase() : base(null) { }

        public MpAvTreeSelectorViewModelBase(P p) : base(p) { }

    }
}
