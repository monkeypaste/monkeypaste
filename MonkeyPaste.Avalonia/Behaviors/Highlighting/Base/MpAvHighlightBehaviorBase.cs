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
        protected MpAvHighlightSelectorBehavior ParentSelector { get; private set; }

        protected abstract MpTextRange ContentRange { get; }

        public abstract MpHighlightType HighlightType { get; }
        public abstract MpContentQueryBitFlags AcceptanceFlags { get; }

        public int Priority => (int)HighlightType;

        public int SelectedIdx {
            get {
                if (ParentSelector == null ||
                    ParentSelector.SelectedHighlight.region != this) {
                    return -1;
                }
                return ParentSelector.SelectedHighlight.idx;
            }
        }

        private int _matchCount = 0;
        public int MatchCount =>
            _matchCount;

        #endregion

        #region Events
        public event EventHandler<int> MatchCountChanged;
        #endregion

        #region Constructors
        public MpAvHighlightBehaviorBase() : base() {
        }

        #endregion

        #region Public Methods

        public void Reset() {
            ClearHighlighting();
            SetMatchCount(0);
            //SelectedIdx = -1;
        }

        public abstract Task FindHighlightingAsync();

        public virtual void ClearHighlighting() { }

        public virtual async Task ApplyHighlightingAsync() {
            await Task.Delay(1);
            int all_region_active_idx =
                ParentSelector == null ? -1 :
                ParentSelector.SelectedHighlightIdx;

            if (AssociatedObject != null &&
                AssociatedObject.DataContext is MpIHighlightTextRangesInfoViewModel htrivm &&
                htrivm.ActiveHighlightIdx != all_region_active_idx) {
                htrivm.ActiveHighlightIdx = all_region_active_idx;
            }
        }

        #endregion

        #region Protected Methods

        protected override void OnAttached() {
            if (this is MpAvContentWebViewHighlightBehavior) {

            }
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
            DetachFromSelectorAsync().FireAndForgetSafeAsync();
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
            ParentSelector = await FindSelectorAsync();
            if (ParentSelector == null) {
                return;
            }
            ParentSelector.AddHighlighter(this);
        }
        protected async Task DetachFromSelectorAsync() {
            var hsb = ParentSelector ?? await FindSelectorAsync();
            ParentSelector = null;
            if (hsb == null) {
                return;
            }
            hsb.RemoveHighlighter(this);
        }

        protected async Task<MpAvHighlightSelectorBehavior> FindSelectorAsync(int timeout = 10_000) {
            await Task.Delay(0);
            if (AssociatedObject == null ||
                AssociatedObject.GetLogicalAncestors().OfType<MpAvClipTileView>().FirstOrDefault()
                    is not MpAvClipTileView ctv) {
                return null;
            }
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
