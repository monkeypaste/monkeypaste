using MonkeyPaste.Common;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste {
    public abstract class MpMultiSelectorViewModelBase<P, C> :
        MpSelectorViewModelBase<P, C>,
        MpIMultiSelectableViewModel<C>
        where P : class
        where C : class, MpISelectableViewModel {

        public MpMultiSelectorViewModelBase() : base(null) { }

        public MpMultiSelectorViewModelBase(P p) : base(p) { }

        public override C SelectedItem {
            get => PrimaryItem;
            //set => base.SelectedItem = value; 
        }

        public virtual C PrimaryItem { get; }

        public virtual IList<C> SelectedItems {
            get => Items.Where(x => x.IsSelected).ToList();
            set => Items.ForEach(x => x.IsSelected = value == null ? false : value.Contains(x));
        }
    }
}
