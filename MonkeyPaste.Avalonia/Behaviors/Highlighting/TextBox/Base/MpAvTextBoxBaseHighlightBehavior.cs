

using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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


        public override async Task FindHighlightingAsync() {
            await Task.Delay(1);
            _matches.Clear();

            bool can_match = CanMatch();

            if (AssociatedObject != null &&
                AssociatedObject is TextBox tb &&
                AssociatedObject.DataContext is MpIHighlightTextRangesInfoViewModel htrivm) {
                var to_remove = htrivm.HighlightRanges.Where(x => x.Document == ContentRange.Document).ToList();
                for (int i = 0; i < to_remove.Count; i++) {
                    htrivm.HighlightRanges.Remove(to_remove[i]);
                }
                if (can_match) {

                    _matches.AddRange(
                        Mp.Services.Query.Infos
                        .Where(x => !string.IsNullOrEmpty(x.MatchValue) && x.QueryFlags.HasStringMatchFilterFlag())
                        .SelectMany(x => tb.Text.QueryText(x))
                        .Select(x => new MpTextRange(ContentRange.Document, x.Item1, x.Item2))
                        .Distinct()
                        .OrderBy(x => x.StartIdx)
                        .ThenBy(x => x.Count));

                    foreach (var m in _matches) {
                        htrivm.HighlightRanges.Add(m);
                    }
                }
            }
            SetMatchCount(_matches.Count);
        }
    }
}
