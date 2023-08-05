using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {

    public partial class MpAvPopupMenuSelector : MpAvUserControl<MpAvIPopupSelectorMenuViewModel> {

        public MpAvPopupMenuSelector() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
