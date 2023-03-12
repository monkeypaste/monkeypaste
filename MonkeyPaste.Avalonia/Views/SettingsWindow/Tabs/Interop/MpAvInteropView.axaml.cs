using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvInteropView : MpAvUserControl<MpAvSettingsViewModel> {
        public MpAvInteropView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
