using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeWindow : MpAvNotificationWindow {
        public MpAvWelcomeWindow() : this(null) { }
        public MpAvWelcomeWindow(Window owner = default) : base(owner) {
            InitializeComponent();
            //this.GetObservable(Window.WidthProperty).Subscribe(paramValue => OnWindowSizeChanged());
            //this.GetObservable(Window.HeightProperty).Subscribe(paramValue => OnWindowSizeChanged());
            this.GetObservable(Window.BoundsProperty).Subscribe(value => OnWindowSizeChanged());
            this.GetObservable(Window.WindowStateProperty).Subscribe(value => OnWindowStateChanged());

            var mb = this.FindControl<Button>("MinimizeButton");
            mb.Click += (s, e) => {
                if (TopLevel.GetTopLevel(this) is Window w) {
                    w.WindowState = WindowState.Minimized;
                }
            };
        }

        private void OnWindowStateChanged() {
            MpAvWindowManager.AllWindows
                    .Where(x => x.DataContext is MpAvPointerGestureWindowViewModel || x.DataContext is MpAvFakeWindowViewModel)
                    .ForEach(x => x.WindowState = WindowState);
        }

        private void OnWindowSizeChanged() {
            if (this.Screens.ScreenFromVisual(this) is not { } s) {
                return;
            }
            double x = (double)s.WorkingArea.Center.X - ((this.Bounds.Width * s.Scaling) / 2d);
            double y = (double)s.WorkingArea.Center.Y - ((this.Bounds.Height * s.Scaling) / 2d);
            this.Position = new PixelPoint((int)x, (int)y);
        }
    }

}
