using Avalonia.Controls;
using Avalonia.Markup.Xaml;
#if PLAT_WV
using AvaloniaWebView;
#endif

namespace MonkeyPaste.Avalonia {
    public partial class MpAvContentWebViewContainer : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvContentWebViewContainer() {
            AvaloniaXamlLoader.Load(this);

#if PLAT_WV
            this.FindControl<MpAvContentWebView>("ContentWebView").WebView = this.FindControl<WebView>("AvWebView"); 
#endif
        }

    }
}
