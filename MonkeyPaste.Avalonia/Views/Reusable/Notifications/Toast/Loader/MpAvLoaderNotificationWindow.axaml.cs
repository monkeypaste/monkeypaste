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

        public override MpAvLoaderNotificationViewModel BindingContext =>
            DataContext as MpAvLoaderNotificationViewModel;
        public MpAvLoaderNotificationWindow() {
            AvaloniaXamlLoader.Load(this);

            this.GetObservable(Window.IsVisibleProperty).Subscribe(value => OnIsVisibleChanged());
        }

        private void OnIsVisibleChanged() {
            if (BindingContext == null || !IsVisible) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                await BindingContext.ProgressLoader.BeginLoaderAsync();
                await BindingContext.ProgressLoader.FinishLoaderAsync();
            });
        }
    }
}
