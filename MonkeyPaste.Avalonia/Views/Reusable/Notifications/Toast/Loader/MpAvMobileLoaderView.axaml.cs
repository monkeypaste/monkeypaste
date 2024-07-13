using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PropertyChanged;
using WebViewCore.Extensions;

namespace MonkeyPaste.Avalonia {

    [DoNotNotify]
    public partial class MpAvMobileLoaderView : MpAvUserControl {
        public MpAvMobileLoaderView() {
            InitializeComponent();
        }
        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            if (DataContext is not MpAvLoaderNotificationViewModel lnvm) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {

                await lnvm.ProgressLoader.BeginLoaderAsync();
                await lnvm.ProgressLoader.FinishLoaderAsync();
            });
        }
    }
}
