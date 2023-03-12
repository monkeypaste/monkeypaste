using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPreferencesView : MpAvUserControl<MpAvSettingsViewModel> {
        public MpAvPreferencesView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
