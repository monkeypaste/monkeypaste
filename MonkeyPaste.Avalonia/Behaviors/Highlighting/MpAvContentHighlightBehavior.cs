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
                    _contentRange = new MpTextRange(wv);
                }
                return _contentRange;
            }
        }

        public override MpHighlightType HighlightType => MpHighlightType.Content;
        public override MpContentQueryBitFlags AcceptanceFlags =>
            MpContentQueryBitFlags.Annotations |
            MpContentQueryBitFlags.Content;
        public override async Task FindHighlightingAsync() {
            int matchCount = 0;
            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv) {
                await wv.PerformLoadContentRequestAsync();
                while (wv.SearchResponse == null) {
                    await Task.Delay(100);
                }
                matchCount = wv.SearchResponse.rangeCount;
                wv.SearchResponse = null;
            }
            SetMatchCount(matchCount);
        }

        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);
            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv) {
                if (SelectedIdx < 0) {
                    wv.SendMessage($"deactivateFindReplace_ext()");
                    return;
                }

                var msg = new MpQuillContentSearchRangeNavigationMessage() {
                    isAbsoluteOffset = true,
                    curIdxOffset = SelectedIdx
                };
                wv.SendMessage($"activateFindReplace_ext('{msg.SerializeJsonObjectToBase64()}')");
            }
        }
        public override void ClearHighlighting() {

            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv) {

                wv.SendMessage($"deactivateFindReplace_ext()");
                wv.PerformLoadContentRequestAsync().FireAndForgetSafeAsync();
            }
        }
    }
}
