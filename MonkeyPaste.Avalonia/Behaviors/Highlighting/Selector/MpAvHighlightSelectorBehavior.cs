﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvHighlightSelectorBehavior : Behavior<MpAvClipTileView> {

        #region Properties

        #region Behaviors

        ObservableCollection<MpIHighlightRegion> Items { get; } = new ObservableCollection<MpIHighlightRegion>();
        IEnumerable<MpIHighlightRegion> SortedItems =>
            Items.OrderBy(x => x.Priority);

        IEnumerable<MpIHighlightRegion> EnabledItems =>
            SortedItems.Where(x => IsRegionEnabled(x));

        IEnumerable<MpIHighlightRegion> DisabledItems =>
            SortedItems.Where(x => !IsRegionEnabled(x));

        MpIHighlightRegion SelectedItem =>
            SelectedHighlighterIdx >= 0 && SelectedHighlighterIdx < Items.Count ?
            Items[SelectedHighlighterIdx] :
            null;

        #endregion

        #region State
        bool IsHighlightDataAvailable {
            get {
                var qi = MpAvQueryViewModel.Instance as MpIQueryInfo;
                while (qi != null) {
                    if (qi.QueryFlags.HasStringMatchFilterFlag() &&
                         !string.IsNullOrEmpty(qi.MatchValue)) {
                        // there's something to highlight
                        return true;
                    }
                    qi = qi.Next;
                }
                return false;
            }
        }
        int SelectedMatchIdx =>
            SelectedItem == null ? -1 : SelectedItem.SelectedIdx;

        int SelectedHighlighterIdx { get; set; } = 0;

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
                case nameof(SelectedHighlighterIdx):
                    UpdateActiveIdx();
                    break;
            }

        }
        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

            PerformHighlighting().FireAndForgetSafeAsync();
        }
        private void AssociatedObject_DataContextChanged(object sender, System.EventArgs e) {
            if (AssociatedObject == null ||
                AssociatedObject.DataContext == null ||
                string.IsNullOrEmpty(MpAvQueryViewModel.Instance.MatchValue)) {
                Reset();
            }

            PerformHighlighting().FireAndForgetSafeAsync();
        }

        private void HighlightBehavior_MatchCountChanged(object sender, int matchCount) {
            UpdateActiveIdx();
            int totalCount = Items.Sum(x => x.MatchCount);
            if (totalCount > 1) {
                MpAvSearchBoxViewModel.Instance.NotifyHasMultipleMatches();
            }
        }

        private void Hlr_SelIdxChanged(object sender, int e) {
            UpdateActiveIdx();
        }
        private async void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {

                case MpMessageType.SelectNextMatch:
                case MpMessageType.SelectPreviousMatch:
                    if (msg == MpMessageType.SelectNextMatch) {
                        SelectNextMatch();
                    } else {
                        SelectPreviousMatch();
                    }
                    break;
                case MpMessageType.RequeryCompleted:
                    Reset();
                    await PerformHighlighting();
                    break;
                case MpMessageType.JumpToIdxCompleted:
                    //await Task.Delay(500);
                    await PerformHighlighting();
                    break;
            }
        }

        #endregion

        private async Task PerformHighlighting() {
            if (!IsHighlightDataAvailable) {
                // avoid long running task on content webview which reloads content
                // when no search info provides highlighting
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
                AssociatedObject.BindingContext == null ||
                !AssociatedObject.BindingContext.IsEditorLoaded) {
                // NOTE this happens when popout is unloading and it has highlighting
                return;
            }

            await Task.WhenAll(
                EnabledItems
                .Select(x => x.FindHighlightingAsync()));

            SelectedHighlighterIdx = 0;
            if (EnabledItems.All(x => x.MatchCount == 0)) {
                return;
            }
            SelectNextMatch();

            await Task.WhenAll(EnabledItems.Select(x => x.ApplyHighlightingAsync()));
        }

        private bool IsRegionEnabled(MpIHighlightRegion hr) {
            return
                Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasAnyFlag(hr.AcceptanceFlags));
        }

        private void Reset() {
            SelectedHighlighterIdx = 0;
            Items.ForEach(x => x.Reset());
        }

        private void UpdateActiveIdx() {
            int cur_idx = IsActive ? 0 : -1;
            if (IsActive) {
                foreach (var hlb in SortedItems) {
                    if (hlb == SelectedItem) {
                        cur_idx += hlb.SelectedIdx;
                        break;
                    } else {
                        cur_idx += hlb.MatchCount;
                    }
                }
            }
            if (AssociatedObject != null &&
                AssociatedObject.DataContext is MpIHighlightTextRangesInfoViewModel htrivm) {
                htrivm.ActiveHighlightIdx = cur_idx;
            }
        }

        #region Selection
        private void SelectNextItem() {
            if (!IsActive) {
                return;
            }
            SelectedItem.SelectedIdx = -1;
            do {
                SelectedHighlighterIdx++;
                if (SelectedHighlighterIdx >= Items.Count) {
                    SelectedHighlighterIdx = 0;
                }
            } while (SelectedItem.MatchCount == 0);

            SelectedItem.SelectedIdx = 0;
        }
        private void SelectPrevItem() {
            if (!IsActive) {
                return;
            }
            SelectedItem.SelectedIdx = -1;
            do {
                SelectedHighlighterIdx--;
                if (SelectedHighlighterIdx < 0) {
                    SelectedHighlighterIdx = Items.Count - 1;
                }
            } while (SelectedItem.MatchCount == 0);

            SelectedItem.SelectedIdx = SelectedItem.MatchCount - 1;
        }
        private void SelectNextMatch() {
            if (!IsActive) {
                return;
            }
            int next_idx = SelectedMatchIdx + 1;
            if (next_idx >= SelectedItem.MatchCount) {
                SelectNextItem();
            } else {
                SelectedItem.SelectedIdx = next_idx;
            }
            EnabledItems.ForEach(x => x.ApplyHighlightingAsync());
        }
        private void SelectPreviousMatch() {
            if (!IsActive) {
                return;
            }
            int prev_idx = SelectedMatchIdx - 1;
            if (prev_idx < 0) {
                SelectPrevItem();
            } else {
                SelectedItem.SelectedIdx = prev_idx;
            }

            EnabledItems.ForEach(x => x.ApplyHighlightingAsync());
        }
        #endregion

        #endregion
    }
}