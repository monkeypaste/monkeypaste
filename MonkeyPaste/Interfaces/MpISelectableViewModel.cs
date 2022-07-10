using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISelectableViewModel : MpIViewModel {
        bool IsSelected { get; set; }
        DateTime LastSelectedDateTime { get; set; }
    }

    public interface MpISelectorItemViewModel<TItemType> where TItemType : MpViewModelBase {
        MpISelectorViewModel<TItemType> Selector { get; }
    }

    //public interface MpISelectorViewModel : MpIViewModel  {
    //    object SelectedItem { get; set; }
    //}

    public interface MpISelectorViewModel<T> : MpIViewModel where T : MpViewModelBase {
        T SelectedItem { get; set; }
        ObservableCollection<T> Items { get; set; }
    }

    public interface MpIPluginComponentViewModel : MpIViewModel {
        public MpPluginComponentBaseFormat ComponentFormat { get; }
    }
}
