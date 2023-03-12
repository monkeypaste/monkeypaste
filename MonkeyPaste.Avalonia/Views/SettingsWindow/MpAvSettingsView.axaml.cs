using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSettingsView : MpAvUserControl<MpAvSettingsViewModel> {
        public MpAvSettingsView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
