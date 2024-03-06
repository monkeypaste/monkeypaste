using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public enum MpHighlightType {
        None = 0,
        Title,
        Source,
        Content
    }
    [DoNotNotify]
    public abstract class MpAvHighlightBehaviorBase<T> :
        Behavior<T>,
        IComparable<MpAvHighlightBehaviorBase<T>>,
        MpIHighlightRegion where T : Control {
        #region Private Variables
        #endregion

        protected static MpTextRangeComparer TextRangeComparer = new MpTextRangeComparer();

        #region Properties

        public bool IsVisible => MatchCount > 0;

        protected abstract MpTextRange ContentRange { get; }

        public abstract MpHighlightType HighlightType { get; }
        public abstract MpContentQueryBitFlags AcceptanceFlags { get; }

        public int Priority => (int)HighlightType;

        private int _selectedIdx = -1;
        public int SelectedIdx {
            get => _selectedIdx;
            set {
                if (SelectedIdx != value) {
                    _selectedIdx = value;
                    SelIdxChanged?.Invoke(this, SelectedIdx);
                }
            }
        }

        private int _matchCount = 0;
        public int MatchCount =>
            _matchCount;

        #endregion

        #region Events
        public event EventHandler<int> MatchCountChanged;
        public event EventHandler<int> SelIdxChanged;
        #endregion

        #region Constructors
        public MpAvHighlightBehaviorBase() : base() {
        }

        #endregion

        #region Public Methods

        public void Reset() {
            ClearHighlighting();
            SetMatchCount(0);
            SelectedIdx = -1;
        }

        public abstract Task FindHighlightingAsync();

        public virtual void ClearHighlighting() { }

        public virtual async Task ApplyHighlightingAsync() {
            await Task.Delay(1);
        }

        #endregion

        #region Protected Methods

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Initialized;
        }

        private void AssociatedObject_Initialized(object sender, EventArgs e) {
            Reset();
            AttachToSelectorAsync().FireAndForgetSafeAsync();
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            Reset();
            DetachToSelectorAsync().FireAndForgetSafeAsync();
        }
        protected void SetMatchCount(int count) {
            bool changed = _matchCount != count;
            _matchCount = count;
            if (changed) {
                // webview sets count silent before finding or it'll confuse selector about total count
                // because textboxes are faster and old webview count still will be set
                MatchCountChanged?.Invoke(this, _matchCount);
            }
        }

        protected void FinishFind(List<MpTextRange> matches) {
            if (AssociatedObject == null ||
                AssociatedObject.DataContext is not MpIHighlightTextRangesInfoViewModel hrivm) {
                return;
            }
            var old_ranges = hrivm.HighlightRanges.Where(x => x.Document == ContentRange.Document).ToList();
            if (old_ranges.Difference(matches, TextRangeComparer).Any()) {
                hrivm.HighlightRanges = new(matches.Union(hrivm.HighlightRanges.Where(x => !old_ranges.Contains(x))));
            }
            SetMatchCount(matches.Count);
        }

        protected async Task AttachToSelectorAsync() {
            var hsb = await FindSelectorAsync();
            if (hsb == null) {
                return;
            }
            hsb.AddHighlighter(this);
        }
        protected async Task DetachToSelectorAsync() {
            var hsb = await FindSelectorAsync();
            if (hsb == null) {
                return;
            }
            hsb.RemoveHighlighter(this);
        }

        protected async Task<MpAvHighlightSelectorBehavior> FindSelectorAsync(int timeout = 10_000) {
            //var sw = Stopwatch.StartNew();
            //while (true) {
            //    if (AssociatedObject != null && AssociatedObject.DataContext != null) {
            //        break;
            //    }
            //    if (sw.ElapsedMilliseconds >= 10_000) {
            //        break;
            //    }
            //    await Task.Delay(100);
            //}
            //MpAvClipTileView ctv = null;
            //var parent = AssociatedObject.Parent;
            //while (true) {
            //    if (parent is MpAvClipTileView parent_ctv) {
            //        ctv = parent_ctv;
            //        break;
            //    }
            //    if (parent == null) {
            //        break;
            //    }
            //    parent = parent.Parent;
            //}
            await Task.Delay(0);
            if (AssociatedObject == null ||
                AssociatedObject.GetLogicalAncestors().OfType<MpAvClipTileView>().FirstOrDefault()
                    is not MpAvClipTileView ctv) {
                return null;
            }
            //}
            //if ( ctv == null) {
            //    return null;
            //}
            if (Interaction.GetBehaviors(ctv).OfType<MpAvHighlightSelectorBehavior>()
                .FirstOrDefault() is MpAvHighlightSelectorBehavior hsb) {
                return hsb;
            }
            return null;
        }

        #endregion

        #region Private Methods
        #endregion

        #region IComparable Implementation

        public int CompareTo(MpAvHighlightBehaviorBase<T> ohltrvm) {
            // return -1 if this instance precedes obj
            // return 0 if this instance is obj
            // return 1 if this instance is after obj

            if (ohltrvm == null) {
                return -1;
            }
            if (!ContentRange.IsInSameDocument(ohltrvm.ContentRange)) {
                return ((int)Priority).CompareTo(((int)ohltrvm.Priority));
            }

            //return ContentRange.StartIdx.CompareTo(ohltrvm.ContentRange.StartIdx);
            return ContentRange.CompareTo(ohltrvm.ContentRange);
        }
        #endregion
    }
}
