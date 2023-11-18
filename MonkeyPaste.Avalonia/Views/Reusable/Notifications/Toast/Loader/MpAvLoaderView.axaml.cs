using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvLoaderView : MpAvUserControl<MpAvLoaderNotificationViewModel> {

        public MpAvLoaderView() {
            InitializeComponent();
        }

        protected override void OnLoaded(global::Avalonia.Interactivity.RoutedEventArgs e) {
            base.OnLoaded(e);

            Dispatcher.UIThread.Post(async () => {
                await BindingContext.ProgressLoader.BeginLoaderAsync();
                await BindingContext.ProgressLoader.FinishLoaderAsync();
            });
        }
    }
}

