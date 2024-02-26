using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia;

public partial class MpAvCompositeContentView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvCompositeContentView() {
        InitializeComponent();
    }
}