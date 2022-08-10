using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;
using CefNet.Avalonia;
using Avalonia.Interactivity;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvCefNetContentWebView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvCefNetContentWebView() {
            InitializeComponent();
            var wv = this.FindControl<WebView>("CefNetWebView");
            this.AddHandler(WebView.PointerPressedEvent, Wv_PointerPressed, RoutingStrategies.Tunnel);
            
        }

        private void Wv_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e) {
            e.Handled = BindingContext == null || !BindingContext.IsSubSelectionEnabled;
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
