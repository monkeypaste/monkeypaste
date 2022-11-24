using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileSortFieldView : MpAvUserControl<MpAvClipTileSortFieldViewModel> {
        public MpAvClipTileSortFieldView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
