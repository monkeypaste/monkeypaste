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
    public class MpAvContentTextBoxHighlightBehavior : MpAvHighlightBehaviorBase<Control> {

        protected List<MpTextRange> _matches = new List<MpTextRange>();
        private MpTextRange _contentRange;
        protected override MpTextRange ContentRange {
            get {
                if (_contentRange == null &&
                    AssociatedObject.TryGetVisualDescendant<MpAvContentTextBox>(out var tb)) {
                    _contentRange = new MpTextRange(tb);
                }
                return _contentRange;
            }
        }

        public override MpHighlightType HighlightType => MpHighlightType.Content;
        public override MpContentQueryBitFlags AcceptanceFlags =>
            MpContentQueryBitFlags.Annotations |
            MpContentQueryBitFlags.Content;
        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
            _matches.Clear();

            bool can_match =
                Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasContentMatchFilterFlag());

            if (AssociatedObject != null &&
                AssociatedObject.GetVisualDescendant<TextBox>() is TextBox tb &&
                AssociatedObject.DataContext is MpIHighlightTextRangesInfoViewModel htrivm &&
                can_match) {
                _matches.AddRange(
                    Mp.Services.Query.Infos
                    .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                    .SelectMany(x => tb.Text.QueryText(x))
                    .Select(x => new MpTextRange(ContentRange.Document, x.Item1, x.Item2))
                    .Distinct()
                    .OrderBy(x => x.StartIdx)
                    .ThenBy(x => x.Count));
                htrivm.HighlightRanges.Clear();
                htrivm.HighlightRanges.AddRange(_matches);
            }
            SetMatchCount(_matches.Count);
        }

        public override async Task ApplyHighlightingAsync() {
            await Task.Delay(1);

            if (AssociatedObject.DataContext is not MpIHighlightTextRangesInfoViewModel htrivm) {
                return;
            }
            htrivm.ActiveHighlightIdx = SelectedIdx < 0 ? -1 : SelectedIdx;
        }

        public override void ClearHighlighting() {
            if (AssociatedObject.DataContext is not MpIHighlightTextRangesInfoViewModel htrivm) {
                return;
            }
            htrivm.HighlightRanges.Clear();
            htrivm.ActiveHighlightIdx = -1;
        }
    }
}
