using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using System.Linq;
using System.Reflection;
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
