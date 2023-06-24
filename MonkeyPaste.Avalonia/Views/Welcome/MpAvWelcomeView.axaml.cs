using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvWelcomeView : UserControl {
        public MpAvWelcomeView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
