using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;


namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvContentHighlightBehavior : MpAvHighlightBehaviorBase<Control> {

        private MpTextRange _contentRange;
        protected override MpTextRange ContentRange {
            get {
                if (_contentRange == null &&
                    AssociatedObject.GetVisualDescendant<MpAvContentWebView>() is MpAvContentWebView wv) {
                    _contentRange = new MpTextRange() { Document = wv };
                }
                return _contentRange;
            }
        }

        private int _matchCount;
        public override int MatchCount {
            get => _matchCount;
            protected set => _matchCount = value;
        }

        public override MpHighlightType HighlightType => MpHighlightType.Content;

        public override async Task FindHighlightingAsync() {
            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv) {
                await wv.PerformLoadContentRequestAsync();
                while (wv.SearchResponse == null) {
                    await Task.Delay(100);
                }
                MatchCount = wv.SearchResponse.rangeCount;
                wv.SearchResponse = null;
            }
        }


        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);
            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv) {
                if (SelectedIdx < 0) {
                    wv.ExecuteJavascript($"deactivateFindReplace_ext()");
                    return;
                }

                var msg = new MpQuillContentSearchRangeNavigationMessage() {
                    isAbsoluteOffset = true,
                    curIdxOffset = SelectedIdx
                };
                wv.ExecuteJavascript($"activateFindReplace_ext('{msg.SerializeJsonObjectToBase64()}')");
            }
        }
        public override void ClearHighlighting() {
            base.ClearHighlighting();
            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv) {

                wv.ExecuteJavascript($"deactivateFindReplace_ext()");
                wv.PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
            }
        }
    }
}
