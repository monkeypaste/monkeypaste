using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileTitleView : MpAvUserControl<MpAvClipTileViewModel> {

        public MpAvClipTileTitleView() {
            InitializeComponent();
            var sb = this.FindControl<Button>("ClipTileAppIconImageButton");
            sb.AddHandler(Button.PointerPressedEvent, Sb_PointerPressed, RoutingStrategies.Tunnel);
        }

        private void Sb_PointerPressed(object sender, PointerPressedEventArgs e) {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                BindingContext.GetContentView() is MpIContentView cv) {
                cv.ShowDevTools();
            }
        }
    }
}
