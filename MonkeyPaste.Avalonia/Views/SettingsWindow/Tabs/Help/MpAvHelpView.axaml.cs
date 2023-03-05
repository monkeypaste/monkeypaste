using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvHelpView : MpAvUserControl<MpAvSettingsWindowViewModel> {
        public MpAvHelpView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
