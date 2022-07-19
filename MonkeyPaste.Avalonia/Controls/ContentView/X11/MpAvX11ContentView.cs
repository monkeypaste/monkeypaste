using Avalonia.Controls;
using Avalonia.Media;

namespace MonkeyPaste.Avalonia {
    public class MpAvX11ContentView : MpAvContentViewBase {
        private TextBox _tb;

        public override Control ContentControl => _tb;

        public MpAvX11ContentView() {
            _tb = new TextBox() {
                TextWrapping = TextWrapping.WrapWithOverflow,
                AcceptsReturn = true
            };
        }

        public override void SetContent(string content) {
            _tb.Text = content;
        }
    }
}
