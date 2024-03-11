using Avalonia;
using Avalonia.Xaml.Interactivity;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvHighlightSelectorBehavior : Behavior<MpAvClipTileView> {
        public const int MIN_HL_MATCH_LENGTH = 4;

        #region Properties

        #region Behaviors

        ObservableCollection<MpIHighlightRegion> Items { get; } = [];
        IEnumerable<MpIHighlightRegion> SortedItems =>
            Items.OrderBy(x => x.Priority);

        IEnumerable<MpIHighlightRegion> EnabledItems =>
            SortedItems.Where(x => IsRegionEnabled(x));

        IEnumerable<MpIHighlightRegion> DisabledItems =>
            SortedItems.Where(x => !IsRegionEnabled(x));


        //MpIHighlightRegion SelectedItem =>
        //    SelectedHighlighterIdx >= 0 && SelectedHighlighterIdx < Items.Count ?
        //    Items[SelectedHighlighterIdx] :
        //    null;

        public (MpIHighlightRegion region, int idx)[] Highlights =>
            EnabledItems
            .Where(x => x.MatchCount > 0)
            .SelectMany(x =>
                Enumerable.Range(0, x.MatchCount)
                .Select(y => (x, y)))
            .ToArray();

        public (MpIHighlightRegion region, int idx) SelectedHighlight =>
            SelectedHighlightIdx < 0 || SelectedHighlightIdx >= Highlights.Length ?
                default :
                Highlights[SelectedHighlightIdx];
        #endregion

        #region State
        public int SelectedHighlightIdx { get; set; }

        bool IsActive =>
            Items.Any(x => x.MatchCount > 0);

        #endregion

        #endregion

        #region Constructors

        public MpAvHighlightSelectorBehavior() : base() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            Items.CollectionChanged += Items_CollectionChanged;
            PropertyChanged += MpAvHighlightSelectorBehavior_PropertyChanged;
        }


        #endregion

        #region Public Methods
        public void AddHighlighter(MpIHighlightRegion hlr) {
            if (Items.Contains(hlr)) {
                return;
            }
            Items.Add(hlr);
            hlr.MatchCountChanged += HighlightBehavior_MatchCountChanged;
            hlr.SelIdxChanged += Hlr_SelIdxChanged;
        }


        public void RemoveHighlighter(MpIHighlightRegion hlr) {
            hlr.MatchCountChanged -= HighlightBehavior_MatchCountChanged;
            hlr.SelIdxChanged -= Hlr_SelIdxChanged;
            Items.Remove(hlr);
        }
        #endregion

        #region Protected Methods
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.DataContextChanged += AssociatedObject_DataContextChanged;
        }
        protected override void OnDetaching() {
            if (AssociatedObject != null) {
                AssociatedObject.DataContextChanged -= AssociatedObject_DataContextChanged;
            }
            base.OnDetaching();

            MpMessenger.UnregisterGlobal(ReceivedGlobalMessage);
            Reset();
        }
        #endregion

        #region Private Methods

        #region Event Handlers

        private void MpAvHighlightSelectorBehavior_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e) {
            switch (e.Property.Name) {
                //case nameof(SelectedHighlighterIdx):
                //    UpdateActiveIdx();
                //    break;
            }

        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            PerformHighlighting().FireAndForgetSafeAsync();
        }
        private void AssociatedObject_DataContextChanged(object sender, System.EventArgs e) {
            PerformHighlighting().FireAndForgetSafeAsync();
        }

        private void CheckMatchCount() {
            int totalCount = Items.Where(x => x.MatchCount >= 0).Sum(x => x.MatchCount);
            if (totalCount > 1) {
                MpAvSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
            }
        }
        private void HighlightBehavior_MatchCountChanged(object sender, int matchCount) {
            UpdateActiveIdx();
        }

        private void Hlr_SelIdxChanged(object sender, int e) {
            UpdateActiveIdx();
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {

                case MpMessageType.SelectNextMatch:
                    SelectNextMatchAsync().FireAndForgetSafeAsync();
                    break;
                case MpMessageType.SelectPreviousMatch:
                    SelectPreviousMatchAsync().FireAndForgetSafeAsync();
                    break;
                case MpMessageType.RequeryCompleted:
                case MpMessageType.JumpToIdxCompleted:
                    PerformHighlighting().FireAndForgetSafeAsync();
                    break;
            }
        }

        #endregion

        public bool CanHighlight() {
            return
                Mp.Services != null &&
                Mp.Services.Query != null &&
                Mp.Services.Query.Infos != null &&
                Mp.Services.Query.Infos
                .Any(x =>
                    x.QueryFlags.HasStringMatchFilterFlag() &&
                    x.MatchValue != null &&
                    x.MatchValue.Length >= MIN_HL_MATCH_LENGTH);
        }
        private async Task PerformHighlighting(bool reset = true) {
            if (reset) {
                Reset();
            }
            if (!CanHighlight()) {
                // avoid long running task on content webview which reloads content
                // when no search info provides highlighting
                if (!reset) {
                    Reset();
                }
                return;
            }

            if (AssociatedObject != null &&
                AssociatedObject.BindingContext is MpIAsyncCollectionObject async_dc) {
                while (true) {
                    if (AssociatedObject == null ||
                        AssociatedObject.BindingContext == null) {
                        return;
                    }
                    if (!AssociatedObject.BindingContext.IsAnyBusy) {
                        break;
                    }
                    await Task.Delay(100);
                }
            }

            DisabledItems.ForEach(x => x.Reset());
            if (!EnabledItems.Any()) {
                return;
            }
            if (AssociatedObject == null ||
                AssociatedObject.BindingContext == null) {
                // NOTE this happens when popout is unloading and it has highlighting
                return;
            }

            await Task.WhenAll(
                EnabledItems
                .Select(x => x.FindHighlightingAsync()));

            CheckMatchCount();
            await SelectNextMatchAsync();
            if (EnabledItems.All(x => x.MatchCount == 0)) {
                return;
            }

            await Task.WhenAll(EnabledItems.Select(x => x.ApplyHighlightingAsync()));
        }

        private bool IsRegionEnabled(MpIHighlightRegion hr) {
            return
            Mp.Services.Query.Infos
            .Any(x => x.QueryFlags.HasAnyFlag(hr.AcceptanceFlags) && x.MatchValue.ToStringOrEmpty().Length > MIN_HL_MATCH_LENGTH);
        }

        private void Reset() {
            SelectedHighlightIdx = -1;
            Items.ForEach(x => x.Reset());
        }

        private void UpdateActiveIdx() {
            //PerformHighlighting(false).FireAndForgetSafeAsync();
            EnabledItems.ForEach(x => x.ApplyHighlightingAsync().FireAndForgetSafeAsync());
        }

        #region Selection
        private async Task SelectNextMatchAsync() {
            if (!IsActive) {
                return;
            }
            var prev_hl = SelectedHighlight;
            SelectedHighlightIdx =
                SelectedHighlightIdx < Highlights.Length - 1 ?
                SelectedHighlightIdx + 1 :
                0;
            if (!prev_hl.IsDefault() && prev_hl.region != SelectedHighlight.region) {
                await prev_hl.region.ApplyHighlightingAsync();
            }
            if (!SelectedHighlight.IsDefault()) {
                await SelectedHighlight.region.ApplyHighlightingAsync();
            }

        }
        private async Task SelectPreviousMatchAsync() {
            if (!IsActive) {
                return;
            }
            var prev_hl = SelectedHighlight;
            SelectedHighlightIdx =
                SelectedHighlightIdx > 0 ?
                SelectedHighlightIdx - 1 :
                Highlights.Length - 1;

            if (!prev_hl.IsDefault() && prev_hl.region != SelectedHighlight.region) {
                await prev_hl.region.ApplyHighlightingAsync();
            }
            if (!SelectedHighlight.IsDefault()) {
                await SelectedHighlight.region.ApplyHighlightingAsync();
            }
        }

        #endregion

        #endregion
    }
}
