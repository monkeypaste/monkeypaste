using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIMultiSelectableViewModel<T> where T: MpViewModelBase {
        T PrimaryItem { get; }
        IList<T> SelectedItems { get; }
    }
}
