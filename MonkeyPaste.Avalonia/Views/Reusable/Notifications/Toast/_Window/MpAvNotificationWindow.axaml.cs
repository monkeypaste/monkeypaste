using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvNotificationWindow : MpAvWindow {

        public MpLoaderNotificationViewModel BindingContext => DataContext as MpLoaderNotificationViewModel;
        public MpAvNotificationWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnIsVisibleChanged());
            //this.Opened += MpAvLoaderNotificationWindow_Opened;
        }

        private void MpAvLoaderNotificationWindow_Opened(object sender, EventArgs e) {

        }

        private void OnIsVisibleChanged() {
            if (BindingContext == null) {
                return;
            }
            if (IsVisible) {
                BindingContext.ProgressLoader.BeginLoaderAsync().FireAndForgetSafeAsync(BindingContext);
                Dispatcher.UIThread.Post(async () => {
                    while (true) {
                        BindingContext.OnPropertyChanged(nameof(BindingContext.ProgressBarCurrentWidth));
                        BindingContext.OnPropertyChanged(nameof(BindingContext.Title));
                        BindingContext.OnPropertyChanged(nameof(BindingContext.Body));
                        BindingContext.OnPropertyChanged(nameof(BindingContext.Detail));
                        BindingContext.OnPropertyChanged(nameof(BindingContext.ValueLoaded));
                        if (BindingContext.ValueLoaded >= 100.0d) {
                            await Task.Delay(1000);
                            BindingContext.HideNotification();
                            return;
                        }
                        await Task.Delay(100);
                    }
                });
            } else {
                BindingContext.ProgressLoader.FinishLoaderAsync().FireAndForgetSafeAsync(BindingContext);
            }
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
