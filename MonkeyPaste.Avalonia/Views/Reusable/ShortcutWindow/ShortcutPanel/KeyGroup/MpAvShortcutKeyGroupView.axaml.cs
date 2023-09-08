using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutKeyGroupView : MpAvUserControl<MpAvShortcutKeyGroupViewModel> {
        public MpAvShortcutKeyGroupView() {
            InitializeComponent();
        }
    }
}
