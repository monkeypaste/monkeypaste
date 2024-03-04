using Avalonia.Controls;
using Avalonia.Platform;

namespace MonkeyPaste.Avalonia {
    public class MpAvHiddenWindow : MpAvWindow {

        public MpAvHiddenWindow() : base() {
            Width = 0;
            Height = 0;
            ShowActivated = false;
            Opacity = 0;
            TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent };
            ShowInTaskbar = false;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            SystemDecorations = SystemDecorations.None;
        }

        public void Unhide() {
            Width = 500;
            Height = 500;
            Opacity = 1;
            WindowState = WindowState.Normal;
            Activate();
        }
    }
}
