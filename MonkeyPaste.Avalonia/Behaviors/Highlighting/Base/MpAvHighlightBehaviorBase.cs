using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
namespace MonkeyPaste.Avalonia {
    public enum MpHighlightType {
        None = 0,
        Title,
        Source,
        Content
    }
    [DoNotNotify]
    public abstract class MpAvHighlightBehaviorBase<T> :
        MpAvBehavior<T>,
        IComparable<MpAvHighlightBehaviorBase<T>>, MpIHighlightRegion where T : MpAvUserControl {
        #region Private Variables

        //protected List<KeyValuePair<MpAvTextRange, Brush>> _uniqueContentBackgroundBrushLookup = new List<KeyValuePair<MpAvTextRange, Brush>>();

        protected List<MpAvITextRange> _matches = new List<MpAvITextRange>();

        #endregion

        #region Properties

        public bool IsVisible => MatchCount > 0;

        protected abstract MpAvITextRange ContentRange { get; }

        public abstract MpHighlightType HighlightType { get; }

        public int Priority => (int)HighlightType;

        public int SelectedIdx { get; set; } = -1;

        public int MatchCount => _matches.Count;

        public int ContentItemIdx {
            get {
                if (AssociatedObject == null) {
                    return int.MaxValue;
                }
                if (AssociatedObject.DataContext is MpAvClipTileViewModel civm) {
                    if (HighlightType == MpHighlightType.Content) {
                        return -1;
                    }
                    return -Priority - 1;
                }
                return int.MaxValue;
            }
        }
        //public Brush InactiveOverlayBrush => MpWpfColorHelpers.ChangeBrushAlpha((SolidColorBrush)InactiveHighlightBrush, 128);
        //public Brush ActiveOverlayBrush => MpWpfColorHelpers.ChangeBrushAlpha((SolidColorBrush)ActiveHighlightBrush, 128);

        #region InactiveHighlightBrush Dependency Property

        public IBrush InactiveHighlightBrush {
            get { return (IBrush)GetValue(InactiveHighlightBrushProperty); }
            set { SetValue(InactiveHighlightBrushProperty, value); }
        }

        public static readonly AttachedProperty<IBrush> InactiveHighlightBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "InactiveHighlightBrush",
                Brushes.Pink,
                false);

        #endregion

        #region ActiveHighlightBrush Dependency Property

        public IBrush ActiveHighlightBrush {
            get { return (IBrush)GetValue(ActiveHighlightBrushProperty); }
            set { SetValue(ActiveHighlightBrushProperty, value); }
        }


        public static readonly AttachedProperty<IBrush> ActiveHighlightBrushProperty =
            AvaloniaProperty.RegisterAttached<object, Control, IBrush>(
                "InactiveHighlightBrush",
                Brushes.Crimson,
                false);

        #endregion

        #endregion

        #region Constructors

        protected override void OnLoad() {
            base.OnLoad();

            Dispatcher.UIThread.Post(async () => {
                while (AssociatedObject == null) {
                    await Task.Delay(100);
                }
                //if(!MpSearchBoxViewModel.Instance.HasText) {
                //    return;
                //}
                if (_wasUnloaded) {
                    Reset();
                }
                //UpdateUniqueBackgrounds();
            });
        }

        //protected override void OnUnload() {
        //    base.OnUnload();
        //}
        #endregion

        #region Public Methods

        public abstract Task ScrollToSelectedItemAsync();

        public virtual void Reset() {
            ClearHighlighting();
            _matches.Clear();
            //ReplaceDocumentsBgColors();
        }

        public virtual async Task FindHighlightingAsync() {
            await Task.Delay(5);
            if (AssociatedObject == null || ContentRange == null) {
                // NOTE currently occurs during active search and tag changes
                return;
            }
            string st = MpDataModelProvider.QueryInfo.SearchText;

            //var fd = ContentRange.Start.Parent.FindParentOfType<FlowDocument>();

            //_matches = ContentRange.Start.FindAllText(ContentRange.End, st, MpDataModelProvider.QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive)).ToList();
            bool isCaseSensitive = MpDataModelProvider.QueryInfo.FilterFlags.HasFlag(MpContentFilterType.CaseSensitive);
            bool isWholeWord = MpDataModelProvider.QueryInfo.FilterFlags.HasFlag(MpContentFilterType.WholeWord);
            bool isRegEx = MpDataModelProvider.QueryInfo.FilterFlags.HasFlag(MpContentFilterType.Regex);

            var matchResult = await ContentRange.Start.Document.FindAllTextAsync(st, isCaseSensitive, isWholeWord, isRegEx);
            _matches = matchResult.ToList();

            SelectedIdx = -1;

            if (_matches.Count > 1) {
                MpAvSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
            }
        }

        public virtual void ClearHighlighting() {
            //_matches.ForEach(x => x.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent));
        }

        public virtual void HideHighlighting() {
            ClearHighlighting();
            //ReplaceDocumentsBgColors();
        }

        public virtual async Task ApplyHighlightingAsync() {
            if (_matches.Count == 0) {
                return;
            }
            for (int i = 0; i < _matches.Count; i++) {
                var match = _matches[i];
                IBrush b = i == SelectedIdx ? ActiveHighlightBrush : InactiveHighlightBrush;
                //match.ApplyPropertyValue(TextElement.BackgroundProperty, b);
            }
            AssociatedObject.InvalidateAll();
            await ScrollToSelectedItemAsync();
        }

        //public void UpdateUniqueBackgrounds() {
        //    //called from RtbEditToolbar...
        //    _uniqueContentBackgroundBrushLookup = FindNonTransparentRangeList();
        //    ApplyHighlighting();
        //}
        #endregion

        #region Private Methods

        //private void ReplaceDocumentsBgColors() {
        //    foreach (var kvp in _uniqueContentBackgroundBrushLookup) {
        //        kvp.Key.ApplyPropertyValue(TextElement.BackgroundProperty, kvp.Value);
        //    }
        //}

        #endregion

        #region IComparable Implementation

        public int CompareTo(MpAvHighlightBehaviorBase<T> ohltrvm) {
            // return -1 if this instance precedes obj
            // return 0 if this instance is obj
            // return 1 if this instance is after obj

            if (ohltrvm == null) {
                return -1;
            }
            if (!ContentRange.Start.IsInSameDocument(ohltrvm.ContentRange.Start)) {
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
