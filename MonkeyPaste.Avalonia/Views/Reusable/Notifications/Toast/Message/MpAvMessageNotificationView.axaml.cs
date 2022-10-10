using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvMessageNotificationView : MpAvUserControl<MpMessageNotificationViewModel> {
      
        public MpAvMessageNotificationView() {
            InitializeComponent();
        }


        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
