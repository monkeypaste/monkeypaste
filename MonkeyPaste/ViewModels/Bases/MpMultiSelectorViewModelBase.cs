using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste {
    public abstract class MpMultiSelectorViewModelBase<P, C> :
        MpSelectorViewModelBase<P,C>,
        MpIMultiSelectableViewModel<C> 
        where P : class
        where C : MpISelectableViewModel {

        public MpMultiSelectorViewModelBase() : base(null) { }

        public MpMultiSelectorViewModelBase(P p) : base(p) { }

        public override C SelectedItem {
            get => PrimaryItem;
            //set => base.SelectedItem = value; 
        }

        public virtual C PrimaryItem { get; }

        public virtual IList<C> SelectedItems => Items.Where(x => x.IsSelected).ToList();
    }
}
