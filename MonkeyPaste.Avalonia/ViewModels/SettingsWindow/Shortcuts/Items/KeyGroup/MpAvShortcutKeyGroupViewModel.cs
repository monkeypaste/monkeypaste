using System.Collections.ObjectModel;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutKeyGroupViewModel : MpAvViewModelBase {
        #region Properties

        #region View Models
        public ObservableCollection<MpAvShortcutKeyViewModel> Items { get; set; } = new ObservableCollection<MpAvShortcutKeyViewModel>();

        #endregion

        #region State
        public int SortIdx { get; set; }

        #endregion

        #endregion
        public MpAvShortcutKeyGroupViewModel() { }
    }
}
