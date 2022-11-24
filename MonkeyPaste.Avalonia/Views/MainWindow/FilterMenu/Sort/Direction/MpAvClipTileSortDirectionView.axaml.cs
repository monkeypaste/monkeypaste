using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileSortDirectionView : MpAvUserControl<MpAvClipTileSortDirectionViewModel> {
        public MpAvClipTileSortDirectionView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
