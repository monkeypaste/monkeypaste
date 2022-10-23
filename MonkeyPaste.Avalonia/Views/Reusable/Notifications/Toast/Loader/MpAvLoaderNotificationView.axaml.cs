using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvLoaderNotificationView : MpAvUserControl<MpLoaderNotificationViewModel> {
        private DispatcherTimer _updateTimer;

        public MpAvLoaderNotificationView() {
            InitializeComponent();
            this.AttachedToVisualTree += MpAvLoaderNotificationView_AttachedToVisualTree;
            this.DetachedFromVisualTree += MpAvLoaderNotificationView_DetachedFromVisualTree;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void MpAvLoaderNotificationView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (_updateTimer == null) {
                _updateTimer = new DispatcherTimer();
                _updateTimer.Interval = TimeSpan.FromMilliseconds(100); _updateTimer.IsEnabled = true;
                _updateTimer.Tick += _updateTimer_Tick;
                _updateTimer.Start();
            }
        }

        private async void _updateTimer_Tick(object sender, EventArgs e) {
            BindingContext.OnPropertyChanged(nameof(BindingContext.ProgressBarCurrentWidth));
            BindingContext.OnPropertyChanged(nameof(BindingContext.Title));
            BindingContext.OnPropertyChanged(nameof(BindingContext.Body));
            BindingContext.OnPropertyChanged(nameof(BindingContext.Detail));
            BindingContext.OnPropertyChanged(nameof(BindingContext.ValueLoaded));
            if(BindingContext.ValueLoaded >= 100.0d) {
                await Task.Delay(1000);
                BindingContext.HideNotification();
            }
        }


        private void MpAvLoaderNotificationView_DetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            if (_updateTimer != null) {
                _updateTimer.Stop();
            }
        }
    }
}
