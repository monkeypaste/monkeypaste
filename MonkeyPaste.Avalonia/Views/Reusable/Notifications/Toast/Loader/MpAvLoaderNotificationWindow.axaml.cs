using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvLoaderNotificationWindow : MpAvNotificationWindow {
        public MpAvLoaderNotificationWindow() {
            InitializeComponent();
        }
        //public MpAvLoaderNotificationWindow(MpAvWindow owner = default) : base(owner) {
        //    InitializeComponent();
        //}

        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);

            Dispatcher.UIThread.Post(async () => {
                if (DataContext is not MpAvLoaderNotificationViewModel lnvm) {
                    return;
                }
                await lnvm.ProgressLoader.BeginLoaderAsync();
                await lnvm.ProgressLoader.FinishLoaderAsync();
            });
        }
    }
}

