using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileCornerButtonsView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileCornerButtonsView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
