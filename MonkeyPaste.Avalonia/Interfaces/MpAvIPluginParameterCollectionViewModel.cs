using System.Collections.Generic;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIPluginParameterCollectionViewModel : MpIViewModel {
        IEnumerable<MpAvPluginParameterViewModelBase> Items { get; }
        MpAvPluginParameterViewModelBase SelectedItem { get; set; }
    }
}
