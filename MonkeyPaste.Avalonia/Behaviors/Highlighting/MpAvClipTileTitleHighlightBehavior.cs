

using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileTitleHighlightBehavior : MpAvHighlightBehaviorBase<Control> {
        protected List<MpTextRange> _matches = new List<MpTextRange>();

        private MpTextRange _contentRange;
        protected override MpTextRange ContentRange {
            get {
                if (_contentRange == null &&
                    AssociatedObject is MpAvMarqueeTextBox mtb) {
                    _contentRange = mtb.ContentRange;
                }
                return _contentRange;
            }
        }

        public override MpHighlightType HighlightType =>
            MpHighlightType.Title;

        public override MpContentQueryBitFlags AcceptanceFlags =>
            MpContentQueryBitFlags.Title;

        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
            _matches.Clear();

            bool can_match =
                Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasTitleMatchFilterFlag());

            if (AssociatedObject is MpAvMarqueeTextBox mtb &&
                can_match) {
                _matches.AddRange(
                    Mp.Services.Query.Infos
                    .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                    .SelectMany(x =>
                        mtb.Text.QueryText(
                            x.MatchValue,
                            x.QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive),
                            x.QueryFlags.HasFlag(MpContentQueryBitFlags.WholeWord),
                            x.QueryFlags.HasFlag(MpContentQueryBitFlags.Regex)))
                    .Select(x => new MpTextRange(ContentRange.Document, x.Item1, x.Item2))
                    .Distinct()
                    .OrderBy(x => x.StartIdx)
                    .ThenBy(x => x.Count));

                if (mtb.HighlightRanges == null) {
                    mtb.HighlightRanges = new ObservableCollection<MpTextRange>();
                }
                mtb.HighlightRanges.Clear();
                mtb.HighlightRanges.AddRange(_matches);
            }
            SetMatchCount(_matches.Count);
        }
        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);

            if (AssociatedObject is MpAvMarqueeTextBox mtb) {
                mtb.ActiveHighlightIdx = SelectedIdx < 0 ? null : SelectedIdx;
                Dispatcher.UIThread.Post(mtb.InvalidateVisual);
            }
        }

        public override void ClearHighlighting() {
            if (AssociatedObject is MpAvMarqueeTextBox mtb) {
                mtb.HighlightRanges = null;
                mtb.ActiveHighlightIdx = null;
                Dispatcher.UIThread.Post(mtb.InvalidateVisual);
            }
        }
    }
}
