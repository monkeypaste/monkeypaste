using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia;

public partial class MpAvImageContentView : MpAvUserControl<MpAvClipTileViewModel> {
    public MpAvImageContentView() {
        InitializeComponent();
    }
}