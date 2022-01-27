using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.Internals;

namespace MonkeyPaste {
    public abstract class MpSelectorViewModelBase<P,C> : 
        MpViewModelBase<P>, 
        MpISelectorViewModel<C> where P:class 
        where C: MpViewModelBase,MpISelectableViewModel {

        public MpSelectorViewModelBase() : base(null) { }

        public MpSelectorViewModelBase(P p):base(p) { }

        public virtual ObservableCollection<C> Items { get; set; } = new ObservableCollection<C>();

        public C SelectedItem {
            get => Items.FirstOrDefault(x => x.IsSelected);
            set {
                if (value != SelectedItem) {
                    Items.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        value.IsSelected = true;
                    }
                }
            }
        }
        public bool HasItems => Items.Count > 0;

        public bool IsAnySelected => SelectedItem != null;
        //public List<C> SelectedItems => Items.Where(x => x.IsSelected).ToList();
    }
}
