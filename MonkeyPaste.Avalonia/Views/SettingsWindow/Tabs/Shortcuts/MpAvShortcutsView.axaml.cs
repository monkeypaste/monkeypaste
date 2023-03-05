using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvShortcutsView : MpAvUserControl<MpAvSettingsWindowViewModel> {
        public MpAvShortcutsView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
