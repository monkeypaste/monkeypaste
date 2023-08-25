using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeWindow : MpAvWindow<MpAvWelcomeNotificationViewModel> {

        public MpAvWelcomeWindow() {
            InitializeComponent();
            var mb = this.FindControl<Button>("MinimizeButton");
            mb.Click += (s, e) => {
                if (TopLevel.GetTopLevel(this) is Window w) {
                    w.WindowState = WindowState.Minimized;
                }
            };
            this.GetObservable(Window.WindowStateProperty).Subscribe(value => OnWindowStateChanged());
        }

        private void OnWindowStateChanged() {
            MpAvWindowManager.AllWindows
                    .Where(x => x.DataContext is MpAvPointerGestureWindowViewModel || x.DataContext is MpAvFakeWindowViewModel)
                    .ForEach(x => x.WindowState = WindowState);
        }

    }

}
