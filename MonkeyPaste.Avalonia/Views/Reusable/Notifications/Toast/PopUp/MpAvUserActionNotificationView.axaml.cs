using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvUserActionNotificationView : MpAvUserControl<MpAvPopUpNotificationViewModel> {

        public MpAvUserActionNotificationView() : base() {
            InitializeComponent();

            var retry_btn = this.FindControl<Button>("RetryButton");
            retry_btn.PointerReleased += Retry_btn_PointerReleased;
        }
        private void Retry_btn_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            this.GetVisualAncestor<Window>().Close();
        }


        private void Fix_btn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            BindingContext.IsFixing = true;
        }
    }

}
