using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPlainTextContentView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvPlainTextContentView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
