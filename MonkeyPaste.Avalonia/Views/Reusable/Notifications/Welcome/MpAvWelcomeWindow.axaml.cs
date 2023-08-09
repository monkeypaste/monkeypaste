using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
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
            int show_count = 0;
            this.GetObservable(IsVisibleProperty).Subscribe(value => {
                if (IsVisible) {
                    show_count++;
                    MpDebug.Assert(show_count < 2, "welcome only once");
                }
            });
            this.Opened += MpAvWelcomeWindow_Opened;
        }

        private void MpAvWelcomeWindow_Opened(object sender, System.EventArgs e) {
        }
    }

}
