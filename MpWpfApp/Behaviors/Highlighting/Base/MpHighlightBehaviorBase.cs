using Microsoft.Xaml.Behaviors;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;

namespace MpWpfApp {
    public enum MpHighlightType {
        None = 0,
        Title,
        Source,
        Content
    }

    public abstract class MpHighlightBehaviorBase<T> : MpBehavior<T>, IComparable<MpHighlightBehaviorBase<T>>, MpIHighlightRegion where T : MpUserControl {
        #region Private Variables

        protected List<KeyValuePair<TextRange, Brush>> _uniqueContentBackgroundBrushLookup = new List<KeyValuePair<TextRange, Brush>>();

        protected List<TextRange> _matches = new List<TextRange>();

        #endregion

        #region Properties

        public bool IsVisible => MatchCount > 0;

        protected abstract TextRange ContentRange { get; }

        public abstract MpHighlightType HighlightType { get; }

        public int Priority => (int)HighlightType;

        public int SelectedIdx { get; set; } = -1;

        public int MatchCount => _matches.Count;

        public int ContentItemIdx {
            get {
                if(AssociatedObject == null) {
                    return int.MaxValue;
                }
                if(AssociatedObject.DataContext is MpContentItemViewModel civm) {
                    if(HighlightType == MpHighlightType.Content) {
                        return -(civm.Parent.ItemViewModels.Count-civm.ItemIdx);
                    }
                    return -Priority - civm.Parent.ItemViewModels.Count;
                }
                return int.MaxValue;
            }
        }

        #region InactiveHighlightBrush Dependency Property

        public Brush InactiveHighlightBrush {
            get { return (Brush)GetValue(InactiveHighlightBrushProperty); }
            set { SetValue(InactiveHighlightBrushProperty, value); }
        }

        public static readonly DependencyProperty InactiveHighlightBrushProperty =
            DependencyProperty.Register(
                nameof(InactiveHighlightBrush), 
                typeof(Brush), 
                typeof(MpHighlightBehaviorBase<T>), 
                new PropertyMetadata(Brushes.Yellow));

        #endregion

        #region ActiveHighlightBrush Dependency Property

        public Brush ActiveHighlightBrush {
            get { return (Brush)GetValue(ActiveHighlightBrushProperty); }
            set { SetValue(ActiveHighlightBrushProperty, value); }
        }

        public static readonly DependencyProperty ActiveHighlightBrushProperty =
            DependencyProperty.Register(
                nameof(ActiveHighlightBrush),
                typeof(Brush),
                typeof(MpHighlightBehaviorBase<T>),
                new PropertyMetadata(Brushes.Pink));

        #endregion

        #endregion

        #region Constructors

        protected override void OnLoad() {
            base.OnLoad();

            MpHelpers.RunOnMainThread(async () => {
                while (AssociatedObject == null) {
                    await Task.Delay(100);
                }
                if(!MpSearchBoxViewModel.Instance.HasText) {
                    return;
                }
                if(_wasUnloaded) {
                    Reset();
                }
                UpdateUniqueBackgrounds();
            });
        }

        //protected override void OnUnload() {
        //    base.OnUnload();
        //}
        #endregion

        #region Public Methods

        public abstract void ScrollToSelectedItem();

        public virtual void Reset() {
            ClearHighlighting();
            ReplaceDocumentsBgColors();
        }

        public virtual async Task FindHighlighting() {
            await Task.Delay(5);
            if(AssociatedObject == null) {
                // NOTE currently occurs during active search and tag changes
                return;
            }
            string st = MpDataModelProvider.QueryInfo.SearchText;

            _matches = MpHelpers.FindStringRangesFromPosition(
                ContentRange.Start,
                st);

            SelectedIdx = -1;

            if (_matches.Count > 1) {
                MpSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
            }
        }

        public void ClearHighlighting() {
            _matches.ForEach(x => x.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent));
        }

        public void HideHighlighting() {
            ClearHighlighting();
            ReplaceDocumentsBgColors();
        }

        public virtual void ApplyHighlighting() {
            for (int i = 0; i < _matches.Count; i++) {
                var match = _matches[i];
                Brush b = i == SelectedIdx ? ActiveHighlightBrush : InactiveHighlightBrush;
                match.ApplyPropertyValue(TextElement.BackgroundProperty, b);
            }
            AssociatedObject.UpdateLayout();
            ScrollToSelectedItem();
        }

        public void UpdateUniqueBackgrounds() {
            //called from RtbEditToolbar...
            _uniqueContentBackgroundBrushLookup = FindNonTransparentRangeList();
            ApplyHighlighting();
        }
        #endregion

        #region Private Methods

        private void ReplaceDocumentsBgColors() {
            foreach (var kvp in _uniqueContentBackgroundBrushLookup) {
                kvp.Key.ApplyPropertyValue(TextElement.BackgroundProperty, kvp.Value);
            }
        }

        private List<KeyValuePair<TextRange, Brush>> FindNonTransparentRangeList() {
            var matchRangeList = new List<KeyValuePair<TextRange, Brush>>();
            //if(HighlightType == MpHighlightType.Source) {
            //    return matchRangeList;
            //}
            if (ContentRange == null) {
                return matchRangeList;
            }

            for (TextPointer position = ContentRange.Start;
              position != null && position.CompareTo(ContentRange.End) <= 0;
              position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    Run run = position.Parent as Run;

                    if (run != null) {
                        if (run.Background != null && run.Background != Brushes.Transparent) {
                            matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(run.ContentStart, run.ContentEnd), run.Background));
                        }
                    } else {
                        Paragraph para = position.Parent as Paragraph;

                        if (para != null) {
                            if (para.Background != null && para.Background != Brushes.Transparent) {
                                matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(para.ContentStart, para.ContentEnd), para.Background));
                            }
                        } else {
                            var span = position.Parent as Span;
                            if (span != null) {
                                if (span.Background != null && span.Background != Brushes.Transparent) {
                                    matchRangeList.Add(new KeyValuePair<TextRange, Brush>(new TextRange(span.ContentStart, span.ContentEnd), span.Background));
                                }
                            }
                        }
                    }
                }
            }
            return matchRangeList;
        }

        #endregion

        #region IComparable Implementation

        public int CompareTo(MpHighlightBehaviorBase<T> ohltrvm) {
            // return -1 if this instance precedes obj
            // return 0 if this instance is obj
            // return 1 if this instance is after obj

            if (ohltrvm == null) {
                return -1;
            }
            if(!ContentRange.Start.IsInSameDocument(ohltrvm.ContentRange.Start)) {
                return ContentItemIdx.CompareTo(ohltrvm.ContentItemIdx);
            }

            return ContentRange.Start.CompareTo(ohltrvm.ContentRange.Start);
            //if (SortOrderIdx < ohltrvm.SortOrderIdx) {
            //    return -1;
            //}
            //if (SortOrderIdx > ohltrvm.SortOrderIdx) {
            //    return 1;
            //}
            //return 0;
        }
        #endregion
    }
}
