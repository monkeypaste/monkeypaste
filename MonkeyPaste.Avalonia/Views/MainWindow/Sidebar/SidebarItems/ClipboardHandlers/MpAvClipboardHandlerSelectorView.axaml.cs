using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipboardHandlerSelectorView : MpAvUserControl<MpAvClipboardHandlerCollectionViewModel> {

        public MpAvClipboardHandlerSelectorView() {
            InitializeComponent();
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
