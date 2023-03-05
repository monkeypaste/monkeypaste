using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvAccountView : MpAvUserControl<MpAvSettingsWindowViewModel> {
        public MpAvAccountView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
