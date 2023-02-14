using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvFilterMenuView : MpAvUserControl<MpAvFilterMenuViewModel> {

        public MpAvFilterMenuView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
