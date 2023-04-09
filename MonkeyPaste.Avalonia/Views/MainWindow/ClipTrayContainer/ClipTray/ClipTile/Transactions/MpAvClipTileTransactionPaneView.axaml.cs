using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileTransactionPaneView : MpAvUserControl<MpAvClipTileTransactionCollectionViewModel> {
        public MpAvClipTileTransactionPaneView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
