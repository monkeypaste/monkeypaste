using System;
using System.Collections.ObjectModel;

namespace MonkeyPaste {
    public interface MpISelectableViewModel : MpIViewModel {
        bool IsSelected { get; set; }
        DateTime LastSelectedDateTime { get; set; }
    }

    public interface MpIConditionalSelectableViewModel : MpISelectableViewModel {
        bool CanSelect { get; }
    }


    public interface MpISelectorViewModel : MpIViewModel {
        object SelectedItem { get; set; }
    }


}
