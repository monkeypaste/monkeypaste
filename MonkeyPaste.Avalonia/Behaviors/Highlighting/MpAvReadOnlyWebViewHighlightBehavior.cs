using Avalonia;
using Avalonia.Threading;
using HtmlAgilityPack;
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
    public class MpAvReadOnlyWebViewHighlightBehavior : MpAvHighlightBehaviorBase<MpAvHtmlPanel> {
        private HtmlDocument _doc;
        private string _plainText;
        private bool _isThisChangingText = false;

        protected List<MpTextRange> _matches = new List<MpTextRange>();

        private List<IDisposable> _disposables = [];
        public override MpHighlightType HighlightType => MpHighlightType.Content;
        public override MpContentQueryBitFlags AcceptanceFlags =>
            MpContentQueryBitFlags.Annotations |
            MpContentQueryBitFlags.Content;

        private MpTextRange _contentRange;
        protected override MpTextRange ContentRange {
            get {
                if (_contentRange == null ||
                    _contentRange.Document != AssociatedObject) {
                    _contentRange = new MpTextRange(AssociatedObject);
                }
                return _contentRange;
            }
        }

        protected override void OnAttached() {
            base.OnAttached();
            if (AssociatedObject is not MpAvHtmlPanel hp) {
                return;
            }
            hp.GetObservable(MpAvHtmlPanel.TextProperty).Subscribe(value => OnTextChaged()).AddDisposable(_disposables);
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            _disposables.ForEach(x => x.Dispose());
            _disposables.Clear();
        }

        private void OnTextChaged() {
            if (_isThisChangingText) {
                _isThisChangingText = false;
                return;
            }
            FindHighlightingAsync().FireAndForgetSafeAsync();
        }
        private bool CanMatch() {
            return Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasContentMatchFilterFlag());
        }

        public override async Task ApplyHighlightingAsync() {
            await base.ApplyHighlightingAsync();
            if (AssociatedObject == null ||
                AssociatedObject.DataContext is not MpAvClipTileViewModel ctvm) {
                return;
            }

            int rel_offset = 0;
            if (ctvm.HighlightRanges.Where(x => x.Document != ContentRange.Document) is { } other_ranges) {
                // should only have these for title or source highlights
                rel_offset = other_ranges.Count();
            }
            int rel_active_idx = SelectedIdx - rel_offset;
            SetActiveMatch(rel_active_idx);
        }

        public override void ClearHighlighting() {
            base.ClearHighlighting();
            _doc = null;
            _plainText = null;
            _matches.Clear();
            if (AssociatedObject == null ||
                AssociatedObject.DataContext is not MpAvClipTileViewModel ctvm) {
                return;
            }
            ctvm.HighlightRanges
                .Where(x => x.Document == ContentRange.Document)
                .ToList()
                .ForEach(x => ctvm.HighlightRanges.Remove(x));
            SetHtml(ctvm.CopyItemData);
            AssociatedObject.ScrollToHome();
        }

        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
            _matches.Clear();

            string plain_html = string.Empty;
            if (AssociatedObject != null &&
                AssociatedObject is MpAvHtmlPanel hp &&
                AssociatedObject.DataContext is MpAvClipTileViewModel ctvm &&
                CanMatch()) {
                plain_html = ctvm.CopyItemData;
                _plainText = ctvm.SearchableText.StripLineBreaks();
                _matches.AddRange(
                        Mp.Services.Query.Infos
                        .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                        .SelectMany(x => _plainText.QueryText(x))
                        .Select(x => new MpTextRange(ContentRange.Document, x.Item1, x.Item2))
                        .Distinct()
                        .OrderBy(x => x.StartIdx)
                        .ThenBy(x => x.Count));
            }

            _doc = ConvertMatchesToHighlights(plain_html);
            FinishFind(_matches);
        }

        private HtmlDocument ConvertMatchesToHighlights(string html) {
            var doc = html.ToHtmlDocument();

            foreach (var match in _matches) {
                string match_text = _plainText.Substring(match.StartIdx, match.Count);
                if (doc.SplitTextRange(match.StartIdx, match.Count, assert_match_text: match_text)
                    is not { } hl_node) {
                    continue;
                }
                hl_node.AddClass("highlight-inactive");
            }

            return doc;
        }

        private void SetActiveMatch(int active_idx) {
            var hl_node_tups = _doc.DocumentNode.SelectNodesSafe($"//span[contains(@class, 'highlight')]").WithIndex();
            MpDebug.Assert(hl_node_tups.Count() == _matches.Count, $"SetActiveMatch count error. Found '{hl_node_tups.Count()}' Expected '{_matches.Count}'", true);

            string active_id = null;
            foreach (var (match_node, idx) in hl_node_tups) {
                match_node.RemoveClass("highlight-inactive");
                match_node.RemoveClass("highlight-active");
                match_node.AddClass(idx == active_idx ? "highlight-active" : "highlight-inactive");
                match_node.Id = $"match{idx}";
                if (idx == active_idx) {
                    active_id = match_node.Id;
                }
            }
            // NOTE changing html resets scroll so store actual scroll pos
            //var cur_offset = AssociatedObject.ScrollOffset;
            void HandleScroll(object sender, EventArgs e) {
                AssociatedObject.LoadComplete -= HandleScroll;

                if (active_id != null) {
                    v
                    AssociatedObject.ScrollToElement(active_id);
                    //Dispatcher.UIThread.Post(async () => {
                    //    await Task.Delay(300);
                    //    AssociatedObject.ScrollToOffset(cur_offset, 0);
                    //    AssociatedObject.ScrollToElement(active_id, 0);
                    //});
                } else {
                    AssociatedObject.ScrollToHome();
                }
            }
            AssociatedObject.LoadComplete += HandleScroll;
            SetHtml(_doc.DocumentNode.OuterHtml);
        }

        private void SetHtml(string html) {
            if (AssociatedObject.Text == html) {
                return;
            }
            _isThisChangingText = true;
            AssociatedObject.SetHtml(html);
        }
    }
}
