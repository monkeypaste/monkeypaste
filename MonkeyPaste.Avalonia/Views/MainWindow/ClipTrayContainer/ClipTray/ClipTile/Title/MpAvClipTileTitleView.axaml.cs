using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileTitleView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileTitleView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void SourceIconGrid_PointerPressed(object sender, PointerPressedEventArgs e) {
            var ctv = this.GetVisualAncestor<MpAvClipTileView>();
            var wv = ctv.GetVisualDescendant<WebViewControl.WebView>();
            wv.ShowDeveloperTools();
        }
    }
}