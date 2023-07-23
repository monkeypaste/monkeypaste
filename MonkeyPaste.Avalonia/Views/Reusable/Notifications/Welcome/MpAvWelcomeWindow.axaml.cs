using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeWindow : MpAvWindow {
        public override MpAvWelcomeNotificationViewModel BindingContext =>
            DataContext as MpAvWelcomeNotificationViewModel;

        public MpAvWelcomeWindow() {
            AvaloniaXamlLoader.Load(this);
            var mb = this.FindControl<Button>("MinimizeButton");
            mb.Click += (s, e) => {
                if (TopLevel.GetTopLevel(this) is Window w) {
                    w.WindowState = WindowState.Minimized;
                }
            };
        }
    }

}
