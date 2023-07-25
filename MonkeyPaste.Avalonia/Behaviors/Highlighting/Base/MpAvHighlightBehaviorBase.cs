using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            AssociatedObject.Initialized += AssociatedObject_Initialized;
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
                MatchCountChanged?.Invoke(this, _matchCount);
            }

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
            var sw = Stopwatch.StartNew();
            while (true) {
                if (AssociatedObject != null && AssociatedObject.DataContext != null) {
                    break;
                }
                if (sw.ElapsedMilliseconds >= 10_000) {
                    break;
                }
                await Task.Delay(100);
            }
            MpAvClipTileView ctv = null;
            var parent = AssociatedObject.Parent;
            while (true) {
                if (parent is MpAvClipTileView parent_ctv) {
                    ctv = parent_ctv;
                    break;
                }
                if (parent == null) {
                    break;
                }
                parent = parent.Parent;
            }
            if (ctv == null) {
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
