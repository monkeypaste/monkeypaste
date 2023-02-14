using Avalonia.Markup.Xaml;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvCefNetContentWebView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvCefNetContentWebView() {

            InitializeComponent();
        }



        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }



    }
}
