using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileDetailItemTipView : MpAvUserControl<MpAvClipTileDetailItemViewModel> {
        public MpAvClipTileDetailItemTipView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
