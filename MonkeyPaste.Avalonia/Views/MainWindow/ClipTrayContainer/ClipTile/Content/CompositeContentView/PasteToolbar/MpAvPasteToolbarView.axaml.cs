using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia;

public partial class MpAvPasteToolbarView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvPasteToolbarView() {
        InitializeComponent();
    }
}