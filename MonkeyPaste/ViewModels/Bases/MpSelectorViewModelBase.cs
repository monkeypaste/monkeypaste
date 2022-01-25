using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Forms.Internals;

namespace MonkeyPaste {
    public class MpSelectorViewModelBase<P,C> : 
        MpViewModelBase<P>, 
        MpISelectorViewModel<C> where P:class 
        where C: MpViewModelBase,MpISelectableViewModel {

        public MpSelectorViewModelBase() : base(null) { }

        public MpSelectorViewModelBase(P p):base(p) { }

        public ObservableCollection<C> Items { get; set; } = new ObservableCollection<C>();

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
    }
}
