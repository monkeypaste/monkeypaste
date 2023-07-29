using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMessageNotificationWindow : MpAvWindow {
        #region Private Variables
        #endregion



        public override MpAvMessageNotificationViewModel BindingContext => DataContext as MpAvMessageNotificationViewModel;
        public MpAvMessageNotificationWindow() {
            AvaloniaXamlLoader.Load(this);
        }

    }

}
