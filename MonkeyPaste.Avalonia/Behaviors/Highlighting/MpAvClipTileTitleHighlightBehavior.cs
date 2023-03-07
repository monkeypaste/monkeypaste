

using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
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
                    _contentRange = new MpTextRange() { Document = mtb };
                }
                return _contentRange;
            }
        }

        public override int MatchCount {
            get => _matches.Count;
            protected set => base.MatchCount = value;
        }

        public override MpHighlightType HighlightType => MpHighlightType.Title;

        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
            _matches.Clear();
            if (AssociatedObject is MpAvMarqueeTextBox mtb) {
                var result = mtb.Text.QueryText(
                    MpAvQueryViewModel.Instance.MatchValue,
                    MpAvQueryViewModel.Instance.QueryFlags.HasFlag(MpContentQueryBitFlags.CaseSensitive),
                    MpAvQueryViewModel.Instance.QueryFlags.HasFlag(MpContentQueryBitFlags.WholeWord),
                    MpAvQueryViewModel.Instance.QueryFlags.HasFlag(MpContentQueryBitFlags.Regex));
                _matches = result.Select(x => new MpTextRange(x.Item1, x.Item2)).ToList();

                if (mtb.HighlightRanges == null) {
                    mtb.HighlightRanges = new ObservableCollection<MpTextRange>();
                }
                mtb.HighlightRanges.Clear();
                mtb.HighlightRanges.AddRange(_matches);
            }
        }
        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);

            if (AssociatedObject is MpAvMarqueeTextBox mtb) {
                mtb.ActiveHighlightIdx = SelectedIdx < 0 ? null : SelectedIdx;
                Dispatcher.UIThread.Post(mtb.InvalidateVisual);
            }
        }

        public override void ClearHighlighting() {
            base.ClearHighlighting();
            if (AssociatedObject is MpAvMarqueeTextBox mtb) {
                mtb.HighlightRanges = null;
                mtb.ActiveHighlightIdx = null;
                Dispatcher.UIThread.Post(mtb.InvalidateVisual);
            }
        }
    }
}
