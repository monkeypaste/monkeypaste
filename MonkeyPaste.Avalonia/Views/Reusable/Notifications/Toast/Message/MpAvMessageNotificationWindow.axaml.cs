using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMessageNotificationWindow : MpAvWindow {
        #region Private Variables
        #endregion



        public MpAvMessageNotificationViewModel BindingContext => DataContext as MpAvMessageNotificationViewModel;
        public MpAvMessageNotificationWindow() {
            AvaloniaXamlLoader.Load(this);
#if DEBUG
            this.AttachDevTools();
#endif
        }

    }

}
