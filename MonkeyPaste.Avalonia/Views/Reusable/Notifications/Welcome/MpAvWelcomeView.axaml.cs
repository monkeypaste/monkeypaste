using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeView : MpAvUserControl<MpAvWelcomeNotificationViewModel> {
        public MpAvWelcomeView() {
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
