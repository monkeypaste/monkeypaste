using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvPluginConfigureView : MpAvUserControl<MpAvPluginItemViewModel> {

        public MpAvPluginConfigureView() {
            InitializeComponent();
        }
    }
}
