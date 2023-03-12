using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPreferenceFrameView : MpAvUserControl<MpAvSettingsFrameViewModel> {
        public MpAvPreferenceFrameView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
