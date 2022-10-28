using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvShortcutKeyGroupViewModel : MpViewModelBase {
        public ObservableCollection<MpAvShortcutKeyViewModel> Items { get; set; } = new ObservableCollection<MpAvShortcutKeyViewModel>();

        public bool IsPlusVisible { get; set; } = false;
        public MpAvShortcutKeyGroupViewModel() { }
    }
}
