using Avalonia;
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
    public class MpAvReadOnlyWebViewHighlightBehavior : MpAvHighlightBehaviorBase<HtmlPanel> {
        private List<IDisposable> _disposables = [];
        public override MpHighlightType HighlightType => MpHighlightType.Content;
        public override MpContentQueryBitFlags AcceptanceFlags =>
            MpContentQueryBitFlags.Annotations |
            MpContentQueryBitFlags.Content; protected List<MpTextRange> _matches = new List<MpTextRange>();

        private MpTextRange _contentRange;
        protected override MpTextRange ContentRange {
            get {
                if (AssociatedObject is MpITextDocumentContainer tdc) {
                    return tdc.ContentRange;
                }

                if (_contentRange == null ||
                    _contentRange.Document != AssociatedObject) {
                    _contentRange = new MpTextRange(AssociatedObject);
                }
                return _contentRange;
            }
        }

        protected override void OnAttached() {
            base.OnAttached();
            if (AssociatedObject is not HtmlPanel hp) {
                return;
            }
            hp.GetObservable(HtmlPanel.TextProperty).Subscribe(value => OnTextChaged()).AddDisposable(_disposables);
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            _disposables.ForEach(x => x.Dispose());
            _disposables.Clear();
        }

        private void OnTextChaged() {
            //FindHighlightingAsync().FireAndForgetSafeAsync();
        }
        private bool CanMatch() {
            return Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasContentMatchFilterFlag());
        }

        public override async Task ApplyHighlightingAsync() {
            await base.ApplyHighlightingAsync();
            if (AssociatedObject == null ||
                AssociatedObject.DataContext is not MpIHighlightTextRangesInfoViewModel htrivm) {
                return;
            }
            htrivm.ActiveHighlightIdx = SelectedIdx;
        }
        public override void ClearHighlighting() {
            base.ClearHighlighting();
            if (AssociatedObject == null ||
                AssociatedObject.DataContext is not MpIHighlightTextRangesInfoViewModel htrivm) {
                return;
            }
            htrivm.HighlightRanges.Clear();
            htrivm.ActiveHighlightIdx = -1;
        }


        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
            _matches.Clear();


            if (AssociatedObject != null &&
                AssociatedObject is HtmlPanel hp &&
                AssociatedObject.DataContext is MpAvClipTileViewModel ctvm) {
                var to_remove = ctvm.HighlightRanges.Where(x => x.Document == ContentRange.Document).ToList();
                for (int i = 0; i < to_remove.Count; i++) {
                    ctvm.HighlightRanges.Remove(to_remove[i]);
                }
                if (CanMatch()) {

                    _matches.AddRange(
                        Mp.Services.Query.Infos
                        .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                        .SelectMany(x => ctvm.SearchableText.StripLineBreaks().QueryText(x))
                        .Select(x => new MpTextRange(ContentRange.Document, x.Item1, x.Item2))
                        .Distinct()
                        .OrderBy(x => x.StartIdx)
                        .ThenBy(x => x.Count));

                    foreach (var m in _matches) {
                        ctvm.HighlightRanges.Add(m);
                    }
                }
            }
            SetMatchCount(_matches.Count);
        }
    }
}
