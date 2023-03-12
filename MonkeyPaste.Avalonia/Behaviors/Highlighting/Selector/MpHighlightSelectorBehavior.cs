using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
    public class MpHighlightSelectorBehavior : Behavior<MpAvClipTileView> {
        #region Properties

        List<MpIHighlightRegion> _items = new List<MpIHighlightRegion>();
        List<MpIHighlightRegion> Items =>
            _items;

        IEnumerable<MpIHighlightRegion> EnabledItems =>
            Items.Where(x => IsRegionEnabled(x));

        IEnumerable<MpIHighlightRegion> DisabledItems =>
            Items.Where(x => !IsRegionEnabled(x));

        MpIHighlightRegion SelectedItem =>
            _selectedHighlighterIdx >= 0 && _selectedHighlighterIdx < Items.Count ?
            Items[_selectedHighlighterIdx] :
            null;

        int SelectedMatchIdx =>
            SelectedItem == null ? -1 : SelectedItem.SelectedIdx;

        private int _selectedHighlighterIdx = 0;

        bool IsActive =>
            Items.Any(x => x.MatchCount > 0);

        #endregion

        #region Protected Methods
        protected override void OnAttached() {
            base.OnAttached();

            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
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
        private void AssociatedObject_DataContextChanged(object sender, System.EventArgs e) {
            if (AssociatedObject == null ||
                AssociatedObject.DataContext == null ||
                string.IsNullOrEmpty(MpAvQueryViewModel.Instance.MatchValue)) {
                Reset();
            }
            if (!Items.Any()) {

                Items.Clear();
                if (AssociatedObject.FindControl<Control>("ClipTileContentView") is Control ctcv) {
                    if (ctcv.FindControl<ContentControl>("ClipTileContentControl") is ContentControl ctcc) {
                        // content
                        Items.Add(Interaction.GetBehaviors(ctcc).FirstOrDefault() as MpIHighlightRegion);

                    }
                }

                if (AssociatedObject.FindControl<MpAvClipTileTitleView>("TileTitleView") is MpAvClipTileTitleView cttv) {
                    if (cttv.FindControl<MpAvMarqueeTextBox>("TileTitleTextBox") is MpAvMarqueeTextBox mtb) {
                        // title
                        Items.Add(Interaction.GetBehaviors(mtb).FirstOrDefault() as MpIHighlightRegion);
                    }
                    if (cttv.FindControl<Button>("ClipTileAppIconImageButton") is Button b) {
                        // source
                        Items.Add(Interaction.GetBehaviors(b).FirstOrDefault() as MpIHighlightRegion);
                    }
                }
                _items = Items.OrderBy(x => x.Priority).ToList();
            }
            PerformHighlighting().FireAndForgetSafeAsync();
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
            if (AssociatedObject != null &&
                AssociatedObject.BindingContext != null) {
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
            if (!AssociatedObject.BindingContext.IsViewLoaded) {
                return;
            }

            await Task.WhenAll(
                EnabledItems
                .Select(x => x.FindHighlightingAsync()));

            _selectedHighlighterIdx = 0;
            if (EnabledItems.All(x => x.MatchCount == 0)) {
                return;
            }
            SelectNextMatch();
            EnabledItems.ForEach(x => x.ApplyHighlightingAsync());
        }

        private bool IsRegionEnabled(MpIHighlightRegion hr) {
            return
                Mp.Services.Query.Infos
                .Any(x => x.QueryFlags.HasAnyFlag(hr.AcceptanceFlags));
        }

        private void Reset() {
            _selectedHighlighterIdx = 0;
            Items.ForEach(x => x.Reset());
        }

        #region Selection
        private void SelectNextItem() {
            if (!IsActive) {
                return;
            }
            SelectedItem.SelectedIdx = -1;
            do {
                _selectedHighlighterIdx++;
                if (_selectedHighlighterIdx >= Items.Count) {
                    _selectedHighlighterIdx = 0;
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
                _selectedHighlighterIdx--;
                if (_selectedHighlighterIdx < 0) {
                    _selectedHighlighterIdx = Items.Count - 1;
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
