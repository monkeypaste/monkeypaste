using System.Collections.Generic;

namespace MonkeyPaste {
    public interface MpIComboBoxItemViewModel : MpIViewModel {
        int IconId { get; }
        string Label { get; }
    }
    public interface MpIAsyncComboBoxItemViewModel : MpIComboBoxItemViewModel {
    }
    public interface MpIAsyncComboBoxViewModel : MpIViewModel {
        IEnumerable<MpIAsyncComboBoxItemViewModel> Items { get; }
        MpIAsyncComboBoxItemViewModel SelectedItem { get; set; }

        bool IsDropDownOpen { get; set; }
    }
}
