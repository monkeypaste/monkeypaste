using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvPreferencesView : MpAvUserControl<MpAvPreferencesMenuViewModel> {
        public MpAvPreferencesView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
