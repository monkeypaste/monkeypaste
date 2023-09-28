using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvLoaderNotificationWindow : MpAvWindow<MpAvLoaderNotificationViewModel> {

        public MpAvLoaderNotificationWindow() {
            InitializeComponent();

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

