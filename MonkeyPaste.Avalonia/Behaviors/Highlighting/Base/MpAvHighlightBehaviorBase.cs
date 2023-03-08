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

        public int SelectedIdx { get; set; } = -1;

        private int _matchCount = 0;
        public int MatchCount =>
            _matchCount;

        #endregion

        #region Constructors
        protected override void OnAttached() {
            base.OnAttached();
            Reset();
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            Reset();
        }
        #endregion

        #region Public Methods

        public void Reset() {
            ClearHighlighting();
            SetMatchCount(0);
            SelectedIdx = -1;
        }

        public abstract Task FindHighlightingAsync();

        public abstract void ClearHighlighting();

        public abstract Task ApplyHighlightingAsync();

        #endregion

        #region Protected Methods

        protected void SetMatchCount(int count) {
            _matchCount = count;
            if (count > 0) {

            }
            if (MatchCount > 1) {
                MpAvSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
            }
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
