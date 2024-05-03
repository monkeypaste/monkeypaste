using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvHiddenWindow : Window {

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
            Bounds = new Rect(Bounds.X, Bounds.Y, Width, Height);
            Opacity = 1;
            WindowState = WindowState.Normal;
            ShowInTaskbar = true;
            ShowActivated = true;
            InvalidateMeasure();
            Show();
            Activate();
        }
    }
}
