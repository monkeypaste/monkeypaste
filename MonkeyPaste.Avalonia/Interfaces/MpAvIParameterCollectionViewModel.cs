using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {

    public interface MpAvIParameterCollectionViewModel : MpISaveOrCancelableViewModel {
        IEnumerable<MpAvParameterViewModelBase> Items { get; }
        MpAvParameterViewModelBase SelectedItem { get; set; }
    }
}
