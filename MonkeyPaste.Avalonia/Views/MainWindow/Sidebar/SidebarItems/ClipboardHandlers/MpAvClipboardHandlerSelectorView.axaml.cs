using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipboardHandlerSelectorView : MpAvUserControl<MpAvClipboardHandlerCollectionViewModel> {

        public MpAvClipboardHandlerSelectorView() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
