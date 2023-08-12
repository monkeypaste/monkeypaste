using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            //int show_count = 0;
            //this.GetObservable(IsVisibleProperty).Subscribe(value => {
            //    if (IsVisible) {
            //        App.Current.SetMainWindow(this);
            //        show_count++;
            //        MpDebug.Assert(show_count < 2, "welcome only once");
            //    }
            //});
            //this.Opened += MpAvWelcomeWindow_Opened;
            //this.Closing += MpAvWelcomeWindow_Closing;
        }

        private void MpAvWelcomeWindow_Closing(object sender, WindowClosingEventArgs e) {
            if (MpAvWindowManager.MainWindow != null) {
                return;
            }
            e.Cancel = true;
            Dispatcher.UIThread.Post(async () => {
                this.Hide();
                while (MpAvWindowManager.MainWindow == null) {
                    await Task.Delay(100);
                }
                this.Close();
            });
        }

        private void MpAvWelcomeWindow_Opened(object sender, System.EventArgs e) {
        }
    }

}
