using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileContentView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileContentView() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
