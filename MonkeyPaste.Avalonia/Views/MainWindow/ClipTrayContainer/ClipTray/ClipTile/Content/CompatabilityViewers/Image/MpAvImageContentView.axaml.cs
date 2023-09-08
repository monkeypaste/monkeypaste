using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia;

public partial class MpAvImageContentView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvImageContentView() {
        InitializeComponent();
    }
}