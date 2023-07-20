using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeWindow : MpAvWindow {
        private PixelPoint? _downPos;
        private PixelPoint _initialWindowPos;
        public MpAvWelcomeNotificationViewModel BindingContext =>
            DataContext as MpAvWelcomeNotificationViewModel;

        public MpAvWelcomeWindow() {
            AvaloniaXamlLoader.Load(this);
            var mb = this.FindControl<Button>("MinimizeButton");
            mb.Click += (s, e) => {
                if (TopLevel.GetTopLevel(this) is Window w) {
                    w.WindowState = WindowState.Minimized;
                }
            };
            this.PointerPressed += MpAvWelcomeView_PointerPressed;
            this.PointerMoved += MpAvWelcomeView_PointerMoved;
            this.PointerReleased += MpAvWelcomeView_PointerReleased;
        }


        private void MpAvWelcomeView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            _downPos = null;
            if (e.Source is Control sc &&
                sc.GetSelfAndVisualAncestors().Any(x => x is Button)) {
                return;
            }
            _initialWindowPos = this.Position;
            _downPos = this.PointToScreen(e.GetPosition(this));
            e.Pointer.Capture(this);
            this.Cursor = new Cursor(StandardCursorType.SizeAll);
        }

        private void MpAvWelcomeView_PointerMoved(object sender, PointerEventArgs e) {
            if (!_downPos.HasValue) {
                return;
            }

            var curMousePos = this.PointToScreen(e.GetPosition(this));
            this.Position = _initialWindowPos + (curMousePos - _downPos.Value);
        }

        private void MpAvWelcomeView_PointerReleased(object sender, PointerReleasedEventArgs e) {
            _downPos = null;
            e.Pointer.Capture(null);
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }
    }

}
