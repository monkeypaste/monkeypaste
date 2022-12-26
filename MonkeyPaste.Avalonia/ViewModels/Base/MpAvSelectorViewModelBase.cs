using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvSelectorViewModelBase<P,C> : 
        MpSelectorViewModelBase<P,C>
        where P : class
        where C : MpISelectableViewModel {

        // BUG error MSB4018: System.ArgumentException: Member 'System.Collections.ObjectModel.ObservableCollection`1' is declared in another module and needs to be imported
        // Must override .net standard observable collection in avalonia for some reason
        public override ObservableCollection<C> Items { get; set; } = new ObservableCollection<C>();

        public MpAvSelectorViewModelBase() : base(null) { }

        public MpAvSelectorViewModelBase(P p) : base(p) { }
    }
}
