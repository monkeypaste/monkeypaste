

using Avalonia.Controls;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvTextBoxBaseHighlightBehavior : MpAvHighlightBehaviorBase<TextBox> {
        protected List<MpTextRange> _matches = new List<MpTextRange>();

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
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e) {
            FindHighlightingAsync().FireAndForgetSafeAsync();
        }

        protected abstract bool CanMatch();

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
                AssociatedObject is TextBox tb &&
                AssociatedObject.DataContext is MpIHighlightTextRangesInfoViewModel htrivm &&
                CanMatch()) {
                _matches.AddRange(
                        Mp.Services.Query.Infos
                        .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                        .SelectMany(x => tb.Text.QueryText(x))
                        .Select(x => new MpTextRange(ContentRange.Document, x.Item1, x.Item2))
                        .Distinct()
                        .OrderBy(x => x.StartIdx)
                        .ThenBy(x => x.Count));
            }
            FinishFind(_matches);
        }
    }
}
