using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Avalonia;


namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvReadOnlyWebViewHighlightBehavior : MpAvContentWebViewHighlightBehavior {
        public override bool IsEnabled =>
            //(ContentWebView == null ||
            // ContentWebView.ReadOnlyWebView == null) ||
            //(ContentWebView != null &&
            //ContentWebView.ReadOnlyWebView != null &&
            //ContentWebView.ReadOnlyWebView.IsVisible);
            true;
        protected override MpAvContentWebView ContentWebView {
            get {
                if (_contentWebView == null && AssociatedObject != null &&
                    AssociatedObject.GetVisualAncestor<UserControl>() is { } uc &&
                    uc.GetVisualDescendant<MpAvContentWebView>() is { } wv) {
                    _contentWebView = wv;
                }
                return _contentWebView;
            }
        }
#if SUGAR_WV
        protected override async Task HandleSearchResponseAsync(MpAvContentWebView wv) {
            await Task.Delay(0);
            string hl_html = wv.SearchResponse.highlightHtmlFragment.ToStringFromBase64();
            SetWebViewHtml(wv, hl_html);
            wv.ReadOnlyWebView.ParseScrollOffsets(wv.SearchResponse.matchOffsetsCsvFragment);
            await base.HandleSearchResponseAsync(wv);
        }

        private void SetWebViewHtml(MpAvContentWebView wv, string html) {
            wv.ReadOnlyWebView.SetCurrentValue(HtmlPanel.TextProperty, html);
            //wv.ReadOnlyWebView.SetHtml(html);
        }
#endif
    }
}
