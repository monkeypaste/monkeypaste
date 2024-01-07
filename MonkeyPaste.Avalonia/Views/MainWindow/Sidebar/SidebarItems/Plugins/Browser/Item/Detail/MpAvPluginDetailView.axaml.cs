using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvPluginDetailView : MpAvUserControl<MpAvPluginItemViewModel> {

        public MpAvPluginDetailView() {
            InitializeComponent();
        }
    }
}
