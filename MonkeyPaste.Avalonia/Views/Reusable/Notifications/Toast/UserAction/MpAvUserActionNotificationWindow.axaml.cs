using Avalonia.Controls;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvUserActionNotificationWindow : MpAvWindow<MpAvUserActionNotificationViewModel> {


        public MpAvUserActionNotificationWindow() : base() {
            Init();
        }
        public MpAvUserActionNotificationWindow(Window owner) : base(owner) {
            Init();
        }

        private void Init() {
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
