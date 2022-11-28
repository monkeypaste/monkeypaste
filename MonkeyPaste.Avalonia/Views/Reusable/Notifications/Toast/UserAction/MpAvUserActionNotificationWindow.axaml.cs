using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.ObjectModel;
using MonkeyPaste;
using MonkeyPaste.Common;
using System.Linq;
using Avalonia.Threading;
using System;
using PropertyChanged;
using System.Collections.Generic;
using System.Diagnostics;
using MonkeyPaste.Common.Avalonia;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvUserActionNotificationWindow : Window {

        public MpUserActionNotificationViewModel BindingContext => DataContext as MpUserActionNotificationViewModel;
        public MpAvUserActionNotificationWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            var retry_btn = this.FindControl<Button>("RetryButton");
            retry_btn.PointerReleased += Retry_btn_PointerReleased;
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
        private void Retry_btn_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e) {
            this.GetVisualAncestor<Window>().Close();
        }


        private void Fix_btn_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            BindingContext.IsFixing = true;
        }
    }

}
