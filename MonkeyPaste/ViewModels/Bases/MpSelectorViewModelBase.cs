using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MonkeyPaste {
    public abstract class MpSelectorViewModelBase<P,C> : 
        MpViewModelBase<P>, 
        MpISelectorViewModel<C> where P:class 
        where C: MpViewModelBase,MpISelectableViewModel {

        public MpSelectorViewModelBase() : base(null) { }

        public MpSelectorViewModelBase(P p):base(p) { }

        public virtual ObservableCollection<C> Items { get; set; } = new ObservableCollection<C>();

        public virtual C SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedItem) {
                    //Items.ForEach(x => x.IsSelected = false);
                    //if (value != null) {
                    //    value.IsSelected = true;
                    //}
                    Items.ForEach(x => x.IsSelected = x == value);
                }
            }
        }

        public virtual C LastSelectedItem => Items.Aggregate((a, b) => a.LastSelectedDateTime > b.LastSelectedDateTime ? a : b);
        public bool HasItems => Items.Count > 0;

        public bool IsAnySelected => SelectedItem != null;
        //public List<C> SelectedItems => Items.Where(x => x.IsSelected).ToList();
    }

    public abstract class MpMultiSelectorViewModelBase<P, C> :
        MpSelectorViewModelBase<P,C>,
        MpIMultiSelectableViewModel<C> 
        where P : class
        where C : MpViewModelBase, MpISelectableViewModel {

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
