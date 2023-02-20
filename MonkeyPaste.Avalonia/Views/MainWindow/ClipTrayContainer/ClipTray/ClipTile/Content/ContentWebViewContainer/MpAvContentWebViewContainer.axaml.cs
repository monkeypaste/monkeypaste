using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvContentWebViewContainer : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvContentWebViewContainer() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
