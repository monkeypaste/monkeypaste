using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAccountView : MpAvUserControl<MpAvSettingsViewModel> {
        public MpAvAccountView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
