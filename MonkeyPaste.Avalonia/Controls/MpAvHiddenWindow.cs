using Avalonia.Controls;
using Avalonia.Platform;

namespace MonkeyPaste.Avalonia {
    public class MpAvHiddenWindow : MpAvWindow {

        public MpAvHiddenWindow() : base() {
            Width = 0;
            Height = 0;
            Opacity = 0;
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
            ShowInTaskbar = false;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            SystemDecorations = SystemDecorations.None;
        }
    }
}
