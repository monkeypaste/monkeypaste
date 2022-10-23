using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvUserActionNotificationView : MpAvUserControl<MpUserActionNotificationViewModel> {
      
        public MpAvUserActionNotificationView() {
            InitializeComponent();
            var fix_btn = this.FindControl<Button>("FixButton");
            fix_btn.Click += Fix_btn_Click;
        }

        private void Fix_btn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            BindingContext.IsFixing = true;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
