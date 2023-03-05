using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvSecurityView : MpAvUserControl<MpAvSettingsWindowViewModel> {
        public MpAvSecurityView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
