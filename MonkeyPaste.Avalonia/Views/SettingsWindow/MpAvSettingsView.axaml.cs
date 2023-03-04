using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSettingsView : MpAvUserControl<MpAvSettingsWindowViewModel> {
        public MpAvSettingsView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
