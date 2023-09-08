using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvShortcutKeyView : MpAvUserControl<MpAvShortcutKeyViewModel> {
        public MpAvShortcutKeyView() {
            InitializeComponent();
        }
    }
}
