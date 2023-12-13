using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvContentWebViewHighlightBehavior : MpAvHighlightBehaviorBase<MpAvContentWebView> {

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
            SetMatchCount(0);
            var sw = Stopwatch.StartNew();
            while (true) {
                if (AssociatedObject == null) {
                    return;
                }
                if (AssociatedObject.IsEditorLoaded) {
                    break;
                }
                if (AssociatedObject.BindingContext.IsPlaceholder) {
                    // this is likely a tile that was filtered out of query
                    // by new search criteria and will block here until
                    // it needs to be used again so vacate
                    return;
                }
                if (sw.ElapsedMilliseconds > 5_000) {
                    // what is the state of the view/view model leading to time out here?
                    return;
                }
                await Task.Delay(100);
            }
            int match_count = 0;
            bool can_match =
                Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasContentMatchFilterFlag());

            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv) {
                await wv.LoadContentAsync(can_match);
                if (can_match) {
                    while (wv.SearchResponse == null) {
                        await Task.Delay(100);
                    }
                    match_count = wv.SearchResponse.rangeCount;
                    wv.SearchResponse = null;
                }
            }
            SetMatchCount(match_count);
        }

        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);
            if (ContentRange != null &&
                ContentRange.Document is MpAvContentWebView wv &&
                wv.IsEditorInitialized) {
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
                ContentRange.Document is MpAvContentWebView wv &&
                wv.IsEditorInitialized) {

                wv.SendMessage($"deactivateFindReplace_ext()");
                wv.LoadContentAsync().FireAndForgetSafeAsync();
            }
        }
    }
}
