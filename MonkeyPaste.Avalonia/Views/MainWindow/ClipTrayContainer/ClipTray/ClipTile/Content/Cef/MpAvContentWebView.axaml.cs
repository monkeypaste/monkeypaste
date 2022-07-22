using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common.Avalonia;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvContentWebView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvContentWebView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void WebView_Navigated(string url, string frameName) {
            var wv = this.GetVisualDescendant<WebViewControl.WebView>();
            if (wv == null) {
                Debugger.Break();
                return;
            }

            string initRequest = @"{
			    envName: 'wpf',
			    isReadOnlyEnabled: true,
			    usedTextTemplates: {},
			    isPasteRequest: false,
			    itemEncodedHtmlData: '" + BindingContext.CopyItemData + "'}";

            wv.ExecuteScriptFunction("init", $"'{BindingContext.CopyItemData}'");

            BindingContext.IsViewLoaded = true;
        }
        private void DbgButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            var wv = this.GetVisualDescendant<WebViewControl.WebView>();
            if (wv == null) {
                Debugger.Break();
                return;
            }
            wv.ShowDeveloperTools();
        }
    }
}
