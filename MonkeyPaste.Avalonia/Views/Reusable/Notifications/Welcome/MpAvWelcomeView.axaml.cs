using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeView : MpAvUserControl<MpAvWelcomeNotificationViewModel> {
        private MpPoint _lastMousePos;
        public MpAvWelcomeView() {
            AvaloniaXamlLoader.Load(this);
            var mb = this.FindControl<Button>("MinimizeButton");
            mb.Click += (s, e) => {
                if (TopLevel.GetTopLevel(this) is Window w) {
                    w.WindowState = WindowState.Minimized;
                }
            };
            this.PointerPressed += MpAvWelcomeView_PointerPressed;
        }

        private void MpAvWelcomeView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if (e.Source != this) {
                _lastMousePos = null;
                return;
            }
            if (TopLevel.GetTopLevel(this) is not Window w) {
                return;
            }
            double pd = w.VisualPixelDensity();
            this.DragCheckAndStart(e,
                (e1) => {
                    _lastMousePos = e1.GetPosition(this).ToPortablePoint();
                    e1.Pointer.Capture(this);
                    this.Cursor = new Cursor(StandardCursorType.SizeAll);
                },
                (e2) => {
                    if (_lastMousePos == null) {
                        return;
                    }
                    var curMousePos = e2.GetPosition(this).ToPortablePoint();
                    w.Position = (w.Position.ToPortablePoint(pd) + (curMousePos - _lastMousePos)).ToAvPixelPoint(pd);
                    _lastMousePos = curMousePos;
                }, (e3) => {
                    _lastMousePos = null;
                    e3.Pointer.Capture(null);
                    this.Cursor = new Cursor(StandardCursorType.Arrow);
                }, MIN_DISTANCE: 1);
        }
    }

}
