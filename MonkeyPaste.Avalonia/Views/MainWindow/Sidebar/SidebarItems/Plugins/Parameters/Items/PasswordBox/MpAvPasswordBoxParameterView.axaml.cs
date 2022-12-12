using Avalonia.Controls;
using Avalonia.Interactivity;
using MonkeyPaste.Common.Avalonia;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPasswordBoxParameterView : MpAvUserControl<MpAvTextBoxParameterViewModel> {
        public MpAvPasswordBoxParameterView() {
            InitializeComponent();
            var pwd_tb = this.FindControl<TextBox>("PasswordTextBox");

            this.AddHandler(PointerPressedEvent, MpAvPasswordBoxParameterView_PointerPressed, RoutingStrategies.Tunnel);
            pwd_tb.AddHandler(TextBox.CopyingToClipboardEvent, Reject_Clipboard_Handler, RoutingStrategies.Tunnel);
            pwd_tb.AddHandler(TextBox.CopyingToClipboardEvent, Reject_Clipboard_Handler, RoutingStrategies.Tunnel);
        }

        private void MpAvPasswordBoxParameterView_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            if(e.IsRightPress(sender as Control)) {
                // disable right-click / context menu for passwords
                e.Handled = true;
                return;
            }
        }

        private void Reject_Clipboard_Handler(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            e.Handled = true;
        }

    }
}
