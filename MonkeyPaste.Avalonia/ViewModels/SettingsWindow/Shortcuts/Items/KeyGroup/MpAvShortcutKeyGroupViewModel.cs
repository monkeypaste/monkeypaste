using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutKeyGroupViewModel : MpViewModelBase {
        public ObservableCollection<MpAvShortcutKeyViewModel> Items { get; set; } = new ObservableCollection<MpAvShortcutKeyViewModel>();

        public bool IsPlusVisible { get; set; } = false;
        public MpAvShortcutKeyGroupViewModel() { }
    }
}
