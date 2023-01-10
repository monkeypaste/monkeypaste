using System.Collections.Generic;

namespace MonkeyPaste.Avalonia { 

    public interface MpAvIParameterCollectionViewModel : MpIViewModel {
       IEnumerable<MpAvParameterViewModelBase> Items { get; }
        MpAvParameterViewModelBase SelectedItem { get; set; }
    }
}
