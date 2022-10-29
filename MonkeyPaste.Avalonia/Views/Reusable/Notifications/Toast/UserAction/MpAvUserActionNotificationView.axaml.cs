using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvUserActionNotificationView : MpAvUserControl<MpUserActionNotificationViewModel> {
      
        public MpAvUserActionNotificationView() {
            InitializeComponent();
            //var fix_btn = this.FindControl<Button>("FixButton");
            //fix_btn.Click += Fix_btn_Click;
            var retry_btn = this.FindControl<Button>("RetryButton");
            retry_btn.PointerReleased += Retry_btn_PointerReleased;
        }

        private void Retry_btn_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            this.GetVisualAncestor<Window>().Close();
        }


        private void Fix_btn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            BindingContext.IsFixing = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
