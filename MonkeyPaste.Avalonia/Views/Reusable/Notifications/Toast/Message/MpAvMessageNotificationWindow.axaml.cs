using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMessageNotificationWindow : MpAvWindow<MpAvMessageNotificationViewModel> {
        #region Private Variables
        #endregion



        public MpAvMessageNotificationWindow() {
            InitializeComponent();
        }

    }

}
