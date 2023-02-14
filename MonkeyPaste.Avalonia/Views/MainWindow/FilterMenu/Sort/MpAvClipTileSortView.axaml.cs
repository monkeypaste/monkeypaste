using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileSortView : MpAvUserControl<MpAvFilterMenuViewModel> {
        public MpAvClipTileSortView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
