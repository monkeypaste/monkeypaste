using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvPluginListItemView : MpAvUserControl<MpAvPluginItemViewModel> {

        public MpAvPluginListItemView() {
            InitializeComponent();
        }
    }
}
