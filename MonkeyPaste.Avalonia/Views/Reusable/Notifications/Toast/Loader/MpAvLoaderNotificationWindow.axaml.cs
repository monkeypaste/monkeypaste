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
    public partial class MpAvLoaderNotificationWindow : MpAvWindow {

        public MpLoaderNotificationViewModel BindingContext => DataContext as MpLoaderNotificationViewModel;
        public MpAvLoaderNotificationWindow() {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif

            this.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnIsVisibleChanged());
        }

        private void OnIsVisibleChanged() {
            if (BindingContext == null) {
                return;
            }
            if (IsVisible) {
                this.Position = MpAvNotificationPositioner.GetSystemTrayWindowPosition(this);
                //BindingContext.ProgressLoader.BeginLoaderAsync().FireAndForgetSafeAsync(BindingContext);
                Dispatcher.UIThread.Post(async () => {
                    await BindingContext.ProgressLoader.BeginLoaderAsync();
                    await BindingContext.ProgressLoader.FinishLoaderAsync();
                    //while (true) {
                    //    BindingContext.OnPropertyChanged(nameof(BindingContext.ProgressBarCurrentWidth));
                    //    BindingContext.OnPropertyChanged(nameof(BindingContext.Title));
                    //    BindingContext.OnPropertyChanged(nameof(BindingContext.Body));
                    //    BindingContext.OnPropertyChanged(nameof(BindingContext.Detail));
                    //    BindingContext.OnPropertyChanged(nameof(BindingContext.ValueLoaded));
                    //    if (BindingContext.ValueLoaded >= 100.0d) {
                    //        BindingContext.ProgressLoader.OnPropertyChanged(nameof(BindingContext.ProgressLoader.ShowSpinner));
                    //        BindingContext.OnPropertyChanged(nameof(BindingContext.ProgressLoader.Detail));

                    //        //await Task.Delay(1000);
                    //        BindingContext.HideNotification();
                    //        //await BindingContext.ProgressLoader.FinishLoaderAsync();
                    //        return;
                    //    }
                    //    await Task.Delay(100);
                    //}
                });
            } else {
                //BindingContext.ProgressLoader.FinishLoaderAsync().FireAndForgetSafeAsync(BindingContext);
            }
        }
    }
}
