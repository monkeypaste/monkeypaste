using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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
        private double _scrollTimeS = 0.25d;
        HtmlDocument _doc { get; set; }
        string _plainText { get; set; }
        bool _isThisChangingText { get; set; }

        protected List<MpTextRange> _matches = new List<MpTextRange>();
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
            hp.GetObservable(MpAvHtmlPanel.TextProperty).Subscribe(value => OnTextChaged()).AddDisposable(this);
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            this.ClearDisposables();
        }

        private void OnTextChaged() {
            if (_isThisChangingText) {
                _isThisChangingText = false;
                return;
            }
            if (!CanMatch()) {
                return;
            }
            FindHighlightingAsync().FireAndForgetSafeAsync();

            //Dispatcher.UIThread.Post(async () => {
            //    await FindHighlightingAsync();
            //    await ApplyHighlightingAsync();
            //});
        }
        private bool CanMatch() {
            return
                ParentSelector != null &&
                ParentSelector.CanHighlight() &&
                Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasContentMatchFilterFlag());
        }

        public override async Task ApplyHighlightingAsync() {
            await base.ApplyHighlightingAsync();

            var hl_node_tups = _doc.DocumentNode.SelectNodesSafe($"//span[contains(@class, 'highlight')]").WithIndex();
            if (hl_node_tups.Count() != _matches.Count &&
                _matches.Any() &&
                AssociatedObject != null
                && AssociatedObject.DataContext is MpAvClipTileViewModel ctvm) {
                MpConsole.WriteLine($"SetActiveMatch count error #1. Found '{hl_node_tups.Count()}' Expected '{_matches.Count}'", true);
                // BUG sometimes highlights don't show up
                _doc = ConvertMatchesToHighlights(ctvm.CopyItemData);
                hl_node_tups = _doc.DocumentNode.SelectNodesSafe($"//span[contains(@class, 'highlight')]").WithIndex();
            }
            //MpDebug.Assert(hl_node_tups.Count() == _matches.Count, $"SetActiveMatch count error #2. Found '{hl_node_tups.Count()}' Expected '{_matches.Count}'", false, true);

            string active_id = null;
            foreach (var (match_node, idx) in hl_node_tups) {
                match_node.RemoveClass("highlight-inactive");
                match_node.RemoveClass("highlight-active");
                match_node.AddClass(idx == SelectedIdx ? "highlight-active" : "highlight-inactive");
                match_node.Id = $"match{idx}";
                if (idx == SelectedIdx) {
                    active_id = match_node.Id;
                }
            }

            await SetHtmlAsync(_doc.DocumentNode.OuterHtml);

            if (SelectedIdx >= 0 && active_id != null) {
                await AssociatedObject.ScrollToElementAsync(active_id, _scrollTimeS);
                //AssociatedObject.ScrollToElement($"match-{SelectedIdx}");
            } else {
                await AssociatedObject.ScrollToHomeAsync(_scrollTimeS);
            }
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

            Dispatcher.UIThread.Post(async () => {
                await SetHtmlAsync(ctvm.CopyItemData);
                if (AssociatedObject == null) {
                    return;
                }
                await AssociatedObject.ScrollToHomeAsync(_scrollTimeS);
            });
        }

        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
            _matches.Clear();

            if (AssociatedObject != null &&
                    AssociatedObject is MpAvHtmlPanel hp &&
                    AssociatedObject.DataContext is MpAvClipTileViewModel ctvm &&
                    !ctvm.IsAnyPlaceholder &&
                    CanMatch()) {
                //await Task.Run(() => {
                _plainText = ctvm.SearchableText.StripLineBreaks();
                _matches.AddRange(
                        Mp.Services.Query.Infos
                        .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                        .SelectMany(x => _plainText.QueryText(x))
                        .Select(x => new MpTextRange(ContentRange.Document, x.Item1, x.Item2))
                        .Distinct()
                        .OrderBy(x => x.StartIdx)
                        .ThenBy(x => x.Count));
                _doc = ConvertMatchesToHighlights(ctvm.CopyItemData);
                //});
            } else {

                _doc = ConvertMatchesToHighlights(string.Empty);
            }

            FinishFind(_matches);
        }

        private HtmlDocument ConvertMatchesToHighlights(string html) {
            var doc = html.ToHtmlDocument();
            //doc.SplitTextRanges(
            //    ranges: _matches.Select(x => (x.StartIdx, x.Count)).ToArray(),
            //    split_class: "highlight-inactive");
            //assert_match_texts: _matches.Select(x => _plainText.Substring(x.StartIdx, x.Count)).ToList());
            foreach (var (match, idx) in _matches.WithIndex()) {
                //string match_text = _plainText.Substring(match.StartIdx, match.Count);
                if (doc.SplitTextRange(match.StartIdx, match.Count)
                    is not { } hl_node) {
                    continue;
                }
                hl_node.Id = $"match-{idx}";
                hl_node.AddClass("highlight-inactive");
            }
            return doc;
        }


        private async Task SetHtmlAsync(string html) {
            if (AssociatedObject == null ||
                AssociatedObject.Text == html) {
                return;
            }
            _isThisChangingText = true;

            bool is_done = false;
            void OnLoadComplete(object sender, EventArgs e) {
                AssociatedObject.LoadComplete -= OnLoadComplete;
                is_done = true;
            }
            AssociatedObject.LoadComplete += OnLoadComplete;
            AssociatedObject.SetHtml(html);
            while (!is_done) {
                await Task.Delay(100);
            }
        }
    }
}
