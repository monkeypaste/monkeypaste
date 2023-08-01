using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia;

public partial class MpAvFileListContentView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvFileListContentView() {
        AvaloniaXamlLoader.Load(this);
    }
}