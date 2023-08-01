using Avalonia;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPlainTextContentView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvPlainTextContentView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
