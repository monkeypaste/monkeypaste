using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {

    public partial class MpAvPopupMenuSelector : MpAvUserControl<MpIPopupSelectorMenuViewModel> {

        public MpAvPopupMenuSelector() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
