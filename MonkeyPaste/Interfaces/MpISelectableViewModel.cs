using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpISelectableViewModel {
        bool IsSelected { get; set; }
    }

    public interface MpISelectorViewModel<T> where T : MpViewModelBase {
        T SelectedItem { get; set; }
        ObservableCollection<T> Items { get; set; }
    }
}
