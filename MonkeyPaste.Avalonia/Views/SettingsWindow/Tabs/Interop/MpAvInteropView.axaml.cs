using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvInteropView : MpAvUserControl<MpAvSettingsWindowViewModel> {
        public MpAvInteropView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
