using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvMessageNotificationWindow : Window {
        #region Private Variables
        #endregion


        public MpMessageNotificationViewModel BindingContext => DataContext as MpMessageNotificationViewModel;
        public MpAvMessageNotificationWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }

}
