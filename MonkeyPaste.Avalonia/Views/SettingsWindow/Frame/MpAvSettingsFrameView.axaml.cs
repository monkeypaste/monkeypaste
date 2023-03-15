using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSettingsFrameView : MpAvUserControl<MpAvSettingsFrameViewModel> {
        public MpAvSettingsFrameView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
