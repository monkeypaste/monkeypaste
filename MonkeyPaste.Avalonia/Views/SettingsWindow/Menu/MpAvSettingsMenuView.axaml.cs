using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvSettingsMenuView : UserControl {
        public MpAvSettingsMenuView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
